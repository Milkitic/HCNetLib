namespace HCNetLib.Stream.Builder.AMD
{
    public class AmdEncoderValue : IEncoderValue
    {
        public AmdEncoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static AmdEncoderValue H264 => new AmdEncoderValue("h264_amf");
    }
}