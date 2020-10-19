using System;
using System.Text;
using HCNetLib.Stream.Builder.NVIDIA;

namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultDecodingSettings : IDecodingSettings
    {
        public Manufacture Manufacture => Manufacture.CPU;

        public DefaultDecoderValue Decoder { get; set; }
        public DefaultDecoderOptions DecoderOptions { get; set; }

        IDecoderValue IDecodingSettings.Decoder => Decoder;
        IDecoderOptions IDecodingSettings.DecoderOptions => DecoderOptions;

        public static DefaultDecodingSettings Default => new DefaultDecodingSettings
        {
            Decoder = DefaultDecoderValue.H264,
            DecoderOptions = DefaultDecoderOptions.Default
        };
    }
}
