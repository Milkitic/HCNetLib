using HCNetLib.Stream.Builder.NVIDIA;

namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultEncoderValue : IEncoderValue
    {
        public DefaultEncoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DefaultEncoderValue H264 => new DefaultEncoderValue("h264");
    }
}