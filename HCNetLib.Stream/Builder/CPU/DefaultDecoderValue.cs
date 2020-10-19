namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultDecoderValue : IDecoderValue
    {
        public DefaultDecoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DefaultDecoderValue H264 => new DefaultDecoderValue("h264");
    }
}