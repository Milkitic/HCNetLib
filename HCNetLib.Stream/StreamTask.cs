using HCNetLib.Stream.Builder;
using HCNetLib.Stream.Builder.AMD;
using HCNetLib.Stream.Builder.CPU;
using HCNetLib.Stream.Builder.Intel;
using HCNetLib.Stream.Builder.NVIDIA;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HCNetLib.Stream
{
    // -hwaccel auto
    // https://blog.csdn.net/xiejiashu/article/details/71786187
    // https://developer.nvidia.com/blog/nvidia-ffmpeg-transcoding-guide/
    // -c:v libx264
    // -c:v h264_nvenc
    // ffmpeg -hide_banner -encoders | Select-String AMD
    // ffmpeg -hide_banner -h encoder=h264_nvenc
    // ffmpeg -hide_banner -h encoder=h264_amf
    public sealed class StreamTask : IEquatable<StreamTask>
    {
        public event Action<StreamTask> ProcessExit;
        private RtspCommandBuilder _builder;
        private readonly StreamManagement _management;
        private Process _proc;
        private bool _isPreparing;
        private TaskCompletionSource<object> _readyTcs;

        public string FilePath => _builder.SavePath;

        public StreamTask(RtspIdentity rtspIdentity, StreamManagement management)
        {
            Identity = rtspIdentity;
            _management = management;
        }

        public RtspIdentity Identity { get; }
        public bool IsRunning => _isPreparing || _proc != null && _proc.Id > 0 && !_proc.HasExited;

        public async Task WaitForReading()
        {
            if (_isPreparing && _readyTcs != null)
                await _readyTcs.Task;
        }

        /// <summary>
        /// async back when start encoding
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public async Task RunAsync(string username, string password, Size resolution)
        {
            if (IsRunning) return;
            _isPreparing = true;
            try
            {
                _readyTcs = new TaskCompletionSource<object>();
                var baseDir = Path.Combine(_management.BaseDir, Identity.Channel.ToString(), ((int)Identity.BitStream).ToString());
                var filePath = Path.Combine(baseDir, "realplay.m3u8");
                if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
                else if (File.Exists(filePath)) File.Delete(filePath);

                _builder = new RtspCommandBuilder()
                    .UseUri(Identity.Host, Identity.Port,
                        HikvisionRouteValue.FromSettings(Identity.Channel, Identity.BitStream))
                    .WithAuthentication(username, password)
                    .WithHlsTime(1)
                    .WithHlsListSize(5)
                    .WithOutputResolution(resolution.Width, resolution.Height)
                    .ToM3U8File(filePath);

                BuildEncoderAndDecoder(resolution);

                if (_builder.EncodingSettings == null)
                    throw new Exception("None of available encoders can be found.");
                if (_builder.DecodingSettings == null)
                    throw new Exception("None of available decoders can be found.");

                var args = _builder.Build();
                WriteInfo("args: " + args);

                _proc = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-loglevel level -hide_banner " + args,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        //WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = false,
                        UseShellExecute = false
                    }
                };

                var o = new Regex(@"\[hls @ (.+)\] Opening '(.+)realplay\.m3u8\.tmp' for writing");

                string errMsg = null;
                string innerErrMsg = null;

                _proc.OutputDataReceived += OnErrorDataReceived;
                _proc.ErrorDataReceived += OnErrorDataReceived;

                _proc.Start();
                _proc.BeginOutputReadLine();
                _proc.BeginErrorReadLine();

                //await Task.Delay(100);
                new Task(() =>
                {
                    _proc.WaitForExit();

                    Thread.Sleep(100);
                    var exitCode = _proc.ExitCode;

                    if (exitCode != 0)
                    {
                        WriteError((innerErrMsg == null
                                ? errMsg
                                : (errMsg == null
                                    ? innerErrMsg
                                    : innerErrMsg + " -> " + errMsg)
                            ) ?? $"Process exited unexpectedly. ({exitCode})");
                        //throw new Exception(errMsg ?? $"Process exited unexpectedly. ({exitCode})");
                    }

                    _builder = null;
                    _proc = null;
                    ProcessExit?.Invoke(this);
                }).Start();

                await _readyTcs.Task;

                void OnErrorDataReceived(object obj, DataReceivedEventArgs e)
                {
                    //Console.WriteLine(e.Data);
                    if (e.Data == null) return;
                    var match = o.Match(e.Data);
                    if (match.Success)
                    {
                        if (_readyTcs.Task?.IsCompleted != true)
                        {
                            ConsoleHelper.WriteInfo("Encoding Started", Module);
                            _readyTcs.SetResult(null);
                        }

                        return;
                    }

                    if (e.Data.StartsWith("[error] ") || e.Data.StartsWith("[fatal] ") || e.Data.StartsWith("[panic] "))
                    {
                        var i = e.Data.IndexOf("] ", StringComparison.Ordinal);
                        errMsg = e.Data.Substring(i + 2);
                        WriteError(errMsg);
                        if (_readyTcs.Task?.IsCompleted != true && !e.Data.StartsWith("[error] "))
                            _readyTcs.SetException(new Exception("Preload exception: " + errMsg));
                        return;
                    }

                    if (e.Data.Contains(" [error] "))
                    {
                        var i = e.Data.IndexOf(" [error] ", StringComparison.Ordinal);
                        innerErrMsg = e.Data.Substring(i + 9);
                        //if (tcs.Task?.IsCompleted != true)
                        //    tcs.SetException(new Exception("Preload exception: " + innerErrMsg));
                    }
                    else if (e.Data.Contains(" [fatal] "))
                    {
                        var i = e.Data.IndexOf(" [fatal] ", StringComparison.Ordinal);
                        innerErrMsg = e.Data.Substring(i + 9);
                        if (_readyTcs.Task?.IsCompleted != true)
                            _readyTcs.SetException(new Exception("Preload exception: " + innerErrMsg));
                    }
                    else if (e.Data.Contains(" [panic] "))
                    {
                        var i = e.Data.IndexOf(" [panic] ", StringComparison.Ordinal);
                        innerErrMsg = e.Data.Substring(i + 9);
                        if (_readyTcs.Task?.IsCompleted != true)
                            _readyTcs.SetException(new Exception("Preload exception: " + innerErrMsg));
                    }
                }
            }
            finally
            {
                _isPreparing = false;
            }
        }

        public async Task StopAsync()
        {
            if (_proc == null || !IsRunning) return;

            var sw = Stopwatch.StartNew();
            using (var cts = new CancellationTokenSource(1000))
            {
                while (sw.ElapsedMilliseconds < 4000)
                {
                    await using (var streamWriter = _proc.StandardInput)
                    {
                        try
                        {
                            streamWriter.Write('q');
                        }
                        catch (Exception e)
                        {
                        }
                    }

                    try
                    {
                        await _proc.WaitForExitAsync(cts.Token);
                        break;
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            }

            if (IsRunning) _proc.Kill();
        }

        private void BuildEncoderAndDecoder(Size resolution)
        {
            var supportedGpuList = GPUSelectHelper.EnumerateSupportedGPU().ToList();
            if (supportedGpuList.Count == 0)
            {
                BuildDefault();
                return;
            }

            var nvList = supportedGpuList.Where(k => k.Manufacture == Manufacture.NVIDIA).ToList();
            var amdList = supportedGpuList.Where(k => k.Manufacture == Manufacture.AMD).ToList();
            var intelList = supportedGpuList.Where(k => k.Manufacture == Manufacture.INTEL).ToList();
            if (resolution.Width <= 480 && intelList.Count > 0)
            {
                var intelGpuCounts = GetUsingIntelCount();
                if (intelGpuCounts >= IntelEncodingSettings.ConcurrentLimit)
                {
                    WriteWarn("Intel encoding concurrent limit reached");
                }
                else
                {
                    BuildIntel(nvList.Count);
                    return;
                }
            }

            if (nvList.Count > 0)
            {
                var nvGpuCounts = GetUsingNvCount();
                if (nvGpuCounts >= NvEncodingSettings.ConcurrentLimit)
                {
                    WriteWarn("NVIDIA encoding concurrent limit reached");
                }
                else
                {
                    BuildNv();
                    return;
                }
            }

            if (amdList.Count > 0)
            {
                var nvGpuCounts = GetUsingAmdCount();
                if (nvGpuCounts >= AmdEncodingSettings.ConcurrentLimit)
                {
                    WriteWarn("AMD encoding concurrent limit reached");
                }
                else
                {
                    BuildAmd(nvList.Count);
                    return;
                }
            }

            if (intelList.Count > 0)
            {
                var intelGpuCounts = GetUsingIntelCount();
                if (intelGpuCounts >= IntelEncodingSettings.ConcurrentLimit)
                {
                    WriteWarn("Intel encoding concurrent limit reached");
                }
                else
                {
                    BuildIntel(nvList.Count);
                    return;
                }
            }

            BuildDefault();
        }

        private void BuildDefault()
        {
            _builder.UseDefaultDecodingSettings(DefaultDecodingSettings.Default);
            _builder.UseDefaultEncodingSettings(DefaultEncodingSettings.Default);
        }

        private int GetUsingNvCount()
        {
            var nvGpuCounts = _management.StreamTasks.Values
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings?.Manufacture == Manufacture.NVIDIA);
            return nvGpuCounts;
        }

        private int GetUsingAmdCount()
        {
            var nvGpuCounts = _management.StreamTasks.Values
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings?.Manufacture == Manufacture.AMD);
            return nvGpuCounts;
        }

        private int GetUsingIntelCount()
        {
            var intelGpuCounts = _management.StreamTasks.Values
                .Where(k => k.IsRunning)
                .Count(k => k._builder?.EncodingSettings?.Manufacture == Manufacture.INTEL);
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
            //if (nvCount > 0)
            //    _builder.UseNvidiaDecodingSettings(NvDecodingSettings.Default);
            //else
            _builder.UseDefaultDecodingSettings(DefaultDecodingSettings.Default);
            _builder.UseIntelEncodingSettings(IntelEncodingSettings.Default)
                .WithHlsTime(2);
        }

        private void WriteInfo(string data)
        {
            ConsoleHelper.WriteInfo(data, Module);
        }

        private void WriteError(string data)
        {
            ConsoleHelper.WriteError(data, Module);
        }

        private void WriteWarn(string data)
        {
            ConsoleHelper.WriteWarn(data, Module);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is StreamTask other && Equals(other);
        }

        public bool Equals(StreamTask other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Identity, other.Identity) && Equals(_builder, other._builder) && Equals(_management, other._management) && Equals(_proc, other._proc);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identity, _builder, _management, _proc);
        }

        public static bool operator ==(StreamTask left, StreamTask right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StreamTask left, StreamTask right)
        {
            return !Equals(left, right);
        }

        internal string Module => $"task @ {Identity}";
    }
}