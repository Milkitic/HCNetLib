using System;
using System.Text;
using HCNetLib.Stream.Builder;

namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultEncodingSettings : IEncodingSettings
    {
        public Manufacture Manufacture => Manufacture.CPU;
        public DefaultEncoderValue Encoder { get; set; }
        public DefaultEncoderOptions EncoderOptions { get; set; }
        IEncoderValue IEncodingSettings.Encoder => Encoder;
        IEncoderOptions IEncodingSettings.EncoderOptions => EncoderOptions;

        public static DefaultEncodingSettings Default => new DefaultEncodingSettings
        {
            Encoder = DefaultEncoderValue.H264,
            EncoderOptions = DefaultEncoderOptions.Default
        };
    }
}
