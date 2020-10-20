namespace HCNetLib.Stream.Builder.AMD
{
    public class AmdEncodingSettings : IEncodingSettings
    {
        public static int ConcurrentLimit { get; set; } = int.MaxValue; // this should not be static but per gpu
        public Manufacture Manufacture => Manufacture.AMD;
        public AmdEncoderValue Encoder { get; set; }
        public AmdEncoderOptions EncoderOptions { get; set; }
        IEncoderValue IEncodingSettings.Encoder => Encoder;
        IEncoderOptions IEncodingSettings.EncoderOptions => EncoderOptions;

        public static AmdEncodingSettings Default => new AmdEncodingSettings
        {
            Encoder = AmdEncoderValue.H264,
            EncoderOptions = AmdEncoderOptions.Default
        };
    }
}