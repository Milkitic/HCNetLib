namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvEncodingSettings : IEncodingSettings
    {
        public static int ConcurrentLimit { get; set; } = 3; // this should not be static but per gpu
        public Manufacture Manufacture => Manufacture.NVIDIA;
        public NvEncoderValue Encoder { get; set; }
        public NvEncoderOptions EncoderOptions { get; set; }
        IEncoderValue IEncodingSettings.Encoder => Encoder;
        IEncoderOptions IEncodingSettings.EncoderOptions => EncoderOptions;

        public static NvEncodingSettings Default => new NvEncodingSettings
        {
            Encoder = NvEncoderValue.H264,
            EncoderOptions = NvEncoderOptions.Default
        };
    }
}