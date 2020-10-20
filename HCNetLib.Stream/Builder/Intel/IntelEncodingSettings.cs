namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelEncodingSettings : IEncodingSettings
    {
        public static int ConcurrentLimit { get; set; } = int.MaxValue; // this should not be static but per gpu
        public Manufacture Manufacture => Manufacture.INTEL;
        public IntelEncoderValue Encoder { get; set; }
        public IntelEncoderOptions EncoderOptions { get; set; }
        IEncoderValue IEncodingSettings.Encoder => Encoder;
        IEncoderOptions IEncodingSettings.EncoderOptions => EncoderOptions;

        public static IntelEncodingSettings Default => new IntelEncodingSettings
        {
            Encoder = IntelEncoderValue.H264,
            EncoderOptions = IntelEncoderOptions.Default
        };
    }
}