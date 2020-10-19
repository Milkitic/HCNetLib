namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvDecoderValue : IDecoderValue
    {
        public NvDecoderValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static NvDecoderValue H264 => new NvDecoderValue("h264_cuvid");
    }
}