namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelEncoderValue : IEncoderValue
    {
        public IntelEncoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static IntelEncoderValue H264 => new IntelEncoderValue("h264_qsv");
    }
}