using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HCNetLib.Stream.Builder;
using HCNetLib.Stream.Builder.CPU;
using HCNetLib.Stream.Builder.NVIDIA;

namespace HCNetLib.Stream
{
    public class StreamManagement
    {
        public string BaseDir { get; set; }
        public List<StreamTask> StreamTasks { get; set; } = new List<StreamTask>();
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

        public async void Run(string username, string password, Size resolution)
        {
            var baseDir = Path.Combine(_baseDir, Channel.ToString(), ((int)BitStream).ToString());
            var filePath = Path.Combine(baseDir, "realplay.m3u8");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
            else if (File.Exists(filePath)) File.Delete(filePath);

            _builder = new RtspCommandBuilder()
                .UseUri(Host, route: HikvisionRouteValue.FromSettings(Channel, BitStream))
                .WithAuthentication(username, password)
                .WithHlsListSize(15).WithHlsTime(1)
                .WithOutputResolution(resolution.Width, resolution.Height)
                .ToM3U8File(filePath);

            var supportedGpuList = GPUSelectHelper.EnumerateSupportedGPU().ToList();
            if (supportedGpuList.Count == 0)
            {
                _builder.UseDefaultDecodingSettings(DefaultDecodingSettings.Default);
                _builder.UseDefaultEncodingSettings(DefaultEncodingSettings.Default);
            }
            else
            {
                var nvList = supportedGpuList.Where(k => k.Manufacture == Manufacture.NVIDIA).ToList();
                if (nvList.Count > 0)
                {
                    var nvGpuCounts =
                        _management.StreamTasks.Count(k => k._builder?.EncodingSettings.Manufacture == Manufacture.NVIDIA);
                    if (nvGpuCounts >= NvEncodingSettings.ConcurrentLimit)
                        throw new NotSupportedException("nv encoding concurrent limit reached");
                    _builder.UseNvidiaDecodingSettings(NvDecodingSettings.Default);
                    _builder.UseNvidiaEncodingSettings(NvEncodingSettings.Default);
                }
            }

            //.UseDecodingSettings(new NvDecodingSettings());
            var o =

            _builder = null;
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

        public void Run()
        {
            if (IsRunning) return;
        }

        public bool IsRunning { get; set; }
    }
}
