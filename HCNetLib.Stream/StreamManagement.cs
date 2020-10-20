using HCNetLib.Stream.Builder;
using HCNetLib.Stream.Builder.AMD;
using HCNetLib.Stream.Builder.CPU;
using HCNetLib.Stream.Builder.Intel;
using HCNetLib.Stream.Builder.NVIDIA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HCNetLib.Stream
{
    public class StreamManagement
    {
        private readonly string _username;
        private readonly string _password;

        public StreamManagement(string baseDir, string username, string password)
        {
            _username = username;
            _password = password;
            BaseDir = baseDir;
        }

        public string BaseDir { get; set; }
        public HashSet<StreamTask> StreamTasks { get; set; } = new HashSet<StreamTask>();

        public void QueueTask(string host, int channel, BitStream bitStream, Size convertResolution)
        {
            var streamTask = new StreamTask(host, channel, bitStream, BaseDir, this);

            if (StreamTasks.TryGetValue(streamTask, out var task))
            {
                if (task.IsRunning) return;
                streamTask = task;
            }
            else
            {
                StreamTasks.Add(streamTask);
            }

            streamTask.Run(_username, _password, convertResolution);
        }
    }

    // -hwaccel auto
    // https://blog.csdn.net/xiejiashu/article/details/71786187
    // https://developer.nvidia.com/blog/nvidia-ffmpeg-transcoding-guide/
    // -c:v libx264
    // -c:v h264_nvenc
    // ffmpeg -hide_banner -encoders | Select-String AMD
    // ffmpeg -hide_banner -h encoder=h264_nvenc
    // ffmpeg -hide_banner -h encoder=h264_amf
    // ffmpeg -hide_banner -rtsp_transport tcp -i rtsp://{user}:{pass}@{host}:554/Streaming/Channels/101 -s 640x480 -force_key_frames "expr: gte(t, n_forced * 3)" -vsync 0 -c:v libx264 -hls_time 3  -hls_list_size 1 -hls_wrap 1 -f hls "E:\m3u8\play.m3u8"
    public sealed class StreamTask
    {
        private readonly string _baseDir;
        private RtspCommandBuilder _builder;
        private readonly StreamManagement _management;
        private Process _proc;

        public StreamTask(string host, int channel, BitStream bitStream, string baseDir, StreamManagement management)
        {
            _baseDir = baseDir;
            _management = management;
            Host = host;
            Channel = channel;
            BitStream = bitStream;
        }

        public string Host { get; }
        public int Channel { get; }
        public BitStream BitStream { get; }

        public bool IsRunning => _proc != null && !_proc.HasExited;

        public async void Run(string username, string password, Size resolution)
        {
            if (IsRunning) return;

            var baseDir = Path.Combine(_baseDir, Channel.ToString(), ((int)BitStream).ToString());
            var filePath = Path.Combine(baseDir, "realplay.m3u8");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
            else if (File.Exists(filePath)) File.Delete(filePath);

            _builder = new RtspCommandBuilder()
                .UseUri(Host, route: HikvisionRouteValue.FromSettings(Channel, BitStream))
                .WithAuthentication(username, password)
                .WithHlsTime(1)
                .WithHlsListSize(5)
                .WithOutputResolution(resolution.Width, resolution.Height)
                .ToM3U8File(filePath);

            var supportedGpuList = GPUSelectHelper.EnumerateSupportedGPU().ToList();
            if (supportedGpuList.Count == 0)
            {
                BuildDefault();
            }
            else
            {
                var nvList = supportedGpuList.Where(k => k.Manufacture == Manufacture.NVIDIA).ToList();
                var amdList = supportedGpuList.Where(k => k.Manufacture == Manufacture.AMD).ToList();
                var intelList = supportedGpuList.Where(k => k.Manufacture == Manufacture.INTEL).ToList();
                if (resolution.Width <= 480 && intelList.Count > 0)
                {
                    var intelGpuCounts = GetUsingIntelCount();
                    if (intelGpuCounts >= IntelEncodingSettings.ConcurrentLimit)
                    {
                        Console.WriteLine("Intel encoding concurrent limit reached");
                    }
                    else
                    {
                        BuildIntel(nvList.Count);
                        goto start_build;
                    }
                }

                if (nvList.Count > 0)
                {
                    var nvGpuCounts = GetUsingNvCount();
                    if (nvGpuCounts >= NvEncodingSettings.ConcurrentLimit)
                    {
                        Console.WriteLine("NVIDIA encoding concurrent limit reached");
                    }
                    else
                    {
                        BuildNv();
                        goto start_build;
                    }
                }

                if (amdList.Count > 0)
                {
                    var nvGpuCounts = GetUsingAmdCount();
                    if (nvGpuCounts >= AmdEncodingSettings.ConcurrentLimit)
                    {
                        Console.WriteLine("AMD encoding concurrent limit reached");
                    }
                    else
                    {
                        BuildAmd(nvList.Count);
                        goto start_build;
                    }
                }

                if (intelList.Count > 0)
                {
                    var intelGpuCounts = GetUsingIntelCount();
                    if (intelGpuCounts >= IntelEncodingSettings.ConcurrentLimit)
                    {
                        Console.WriteLine("Intel encoding concurrent limit reached");
                    }
                    else
                    {
                        BuildIntel(nvList.Count);
                        goto start_build;
                    }
                }

                BuildDefault();
            }

            if (_builder.EncodingSettings == null)
                throw new Exception("None of available encoders can be found.");
            if (_builder.DecodingSettings == null)
                throw new Exception("None of available decoders can be found.");

            start_build:
            var args = _builder.Build();
            Console.WriteLine(args);

            _proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = "-loglevel level -hide_banner " + args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            _proc.OutputDataReceived += OnErrorDataReceived;
            _proc.ErrorDataReceived += OnErrorDataReceived;
            _proc.Start();
            _proc.BeginOutputReadLine();
            _proc.BeginErrorReadLine();

            string errMsg = null;
            await _proc.WaitForExitAsync();
            await Task.Delay(100);
            var exitCode = _proc.ExitCode;

            if (exitCode != 0)
            {
                throw new Exception(errMsg ?? $"Process exited unexpectedly. ({exitCode})");
            }

            _builder = null;
            _proc = null;

            void OnErrorDataReceived(object obj, DataReceivedEventArgs e)
            {
                if (e.Data == null) return;
                errMsg = e.Data;
                if (e.Data.Contains("[error] "))
                    Console.WriteLine($"{Host}/{Channel}0{(int)BitStream + 1}: {e.Data}");
            }
        }

        private void BuildDefault()
        {
            _builder.UseDefaultDecodingSettings(DefaultDecodingSettings.Default);
            _builder.UseDefaultEncodingSettings(DefaultEncodingSettings.Default);
        }

        private int GetUsingNvCount()
        {
            var nvGpuCounts = _management.StreamTasks
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings.Manufacture == Manufacture.NVIDIA);
            return nvGpuCounts;
        }

        private int GetUsingAmdCount()
        {
            var nvGpuCounts = _management.StreamTasks
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings.Manufacture == Manufacture.AMD);
            return nvGpuCounts;
        }

        private int GetUsingIntelCount()
        {
            var intelGpuCounts = _management.StreamTasks
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings.Manufacture == Manufacture.INTEL);
            return intelGpuCounts;
        }

        private void BuildNv()
        {
            _builder.UseNvidiaDecodingSettings(NvDecodingSettings.Default);
            _builder.UseNvidiaEncodingSettings(NvEncodingSettings.Default);
        }

        private void BuildAmd(int nvCount)
        {
            if (nvCount > 0)
                _builder.UseNvidiaDecodingSettings(NvDecodingSettings.Default);
            else
                _builder.UseDecodingSettings(DefaultDecodingSettings.Default);
            _builder.UseAmdEncodingSettings(AmdEncodingSettings.Default);
        }

        private void BuildIntel(int nvCount)
        {
            if (nvCount > 0)
                _builder.UseNvidiaDecodingSettings(NvDecodingSettings.Default);
            else
                _builder.UseIntelDecodingSettings(IntelDecodingSettings.Default);
            _builder.UseIntelEncodingSettings(IntelEncodingSettings.Default)
                .WithHlsTime(2);
        }

        public override bool Equals(object obj)
        {
            return obj is StreamTask st ? this.Equals(st) : base.Equals(obj);
        }

        public bool Equals(StreamTask other)
        {
            return string.Equals(Host, other.Host, StringComparison.OrdinalIgnoreCase) &&
                   Channel == other.Channel &&
                   BitStream == other.BitStream;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Host, StringComparer.OrdinalIgnoreCase);
            hashCode.Add(Channel);
            hashCode.Add(BitStream);
            return hashCode.ToHashCode();
        }
    }
}
