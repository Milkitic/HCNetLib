namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvEncoderValue : IEncoderValue
    {
        public NvEncoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static NvEncoderValue H264 => new NvEncoderValue("h264_nevnc");
    }
}