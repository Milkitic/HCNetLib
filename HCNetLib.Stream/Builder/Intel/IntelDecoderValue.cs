namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelDecoderValue : IDecoderValue
    {
        public IntelDecoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static IntelDecoderValue H264 => new IntelDecoderValue("h264_qsv");
    }
}