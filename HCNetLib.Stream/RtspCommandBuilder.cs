using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HCNetLib.Stream.Builder;

namespace HCNetLib.Stream
{
    public class RtspCommandBuilder : IFFCommandBuilder
    {
        public string RtspHost { get; private set; }
        public int RtspPort { get; private set; } = 554;
        public IRouteValue RtspRoute { get; private set; }
        public IDecodingSettings DecodingSettings { get; private set; }
        public IEncodingSettings EncodingSettings { get; private set; }
        public Size? OutputResolution { get; private set; }
        public int? KeyFrames { get; private set; }
        public int? HlsListSize { get; private set; }
        public NetworkCredential Credential { get; private set; }
        public string SavePath { get; private set; }

        public RtspCommandBuilder UseUri(string rtspHost, int rtspPort = 554, IRouteValue route = null)
        {
            RtspHost = rtspHost;
            RtspPort = rtspPort;
            RtspRoute = route;
            return this;
        }

        public RtspCommandBuilder WithAuthentication(string username, string password)
        {
            Credential = new NetworkCredential(HttpUtility.UrlEncode(username), HttpUtility.UrlEncode(password));
            return this;
        }

        public RtspCommandBuilder WithHlsTime(int count)
        {
            KeyFrames = count;
            return this;
        }

        public RtspCommandBuilder WithHlsListSize(int count)
        {
            HlsListSize = count;
            return this;
        }

        public RtspCommandBuilder WithOutputResolution(int width, int height)
        {
            OutputResolution = new Size(width, height);
            return this;
        }

        public RtspCommandBuilder UseDecodingSettings(IDecodingSettings decodingSettings)
        {
            DecodingSettings = decodingSettings;
            return this;
        }

        public RtspCommandBuilder WithEncodingSettings(IEncodingSettings encodingSettings)
        {
            EncodingSettings = encodingSettings;
            return this;
        }

        /// <summary>
        /// m3u8 file
        /// </summary>
        /// <param name="savePath"></param>
        /// <returns></returns>
        public RtspCommandBuilder ToM3U8File(string savePath)
        {
            SavePath = savePath;
            return this;
        }

        public string Build()
        {
            if (RtspHost == null) throw new Exception("host is null");

            string protocol = "rtsp:";
            var uri = Credential == null
                ? $"{protocol}//{RtspHost}:{RtspPort}{RtspRoute?.Route}"
                : $"{protocol}//{Credential?.UserName}:{Credential?.Password}@{RtspHost}:{RtspPort}{RtspRoute?.Route}";
            var type = "-rtsp_transport tcp";
            var hwaccelCmd = "-hwaccel auto";
            var decoderCmd = DecodingSettings?.Decoder == null
                ? null
                : "-c:v " + DecodingSettings.Decoder?.Value + " ";
            var decoderOptionsCmd = DecodingSettings?.DecoderOptions?.Value;
            var inputCmd = $"-i {uri} ";
            var resolutionCmd = OutputResolution == null
                ? null
                : $"-s {OutputResolution.Value.Width}x{OutputResolution.Value.Height}";
            var keyFrameCmds = KeyFrames == null
                ? null
                : $"-forced-idr 1 " +
                  $"-force_key_frames \"expr: gte(t, n_forced * {KeyFrames.Value})\"";
            var vsyncCmd = "-vsync 0";
            var encoderCmd = EncodingSettings?.Encoder == null
                ? null
                : ("-c:v " + EncodingSettings.Encoder?.Value + " ");
            var hlsTimeCmds = KeyFrames == null
                ? null
                : $"-hls_init_time {KeyFrames.Value} " +
                  $"-hls_time {KeyFrames.Value}";
            var hlsListCmds = HlsListSize == null
                ? null
                : $"-hls_list_size {HlsListSize.Value} " +
                  $"-hls_wrap {HlsListSize.Value}";
            var encoderOptionsCmd = EncodingSettings?.EncoderOptions?.Value;
            //var presetCmd = EncodingSettings?.EncoderPreset == null
            //    ? null
            //    : $"-preset {EncodingSettings.EncoderPreset.Value}";
            var fileCmd = SavePath == null
                ? null
                : $"-f hls \"{SavePath}\"";
            var arr = new[]
            {
                type,
                hwaccelCmd,
                decoderCmd,
                decoderOptionsCmd,
                inputCmd,
                resolutionCmd,
                keyFrameCmds,
                vsyncCmd,
                encoderCmd,
                encoderOptionsCmd,
                hlsTimeCmds,
                hlsListCmds,
                //presetCmd,
                fileCmd
            };
            var args = string.Join(" ", arr.Where(k => k != null));

            return args;
        }
    }
}