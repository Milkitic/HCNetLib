namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelDecodingSettings : IDecodingSettings
    {
        public Manufacture Manufacture => Manufacture.INTEL;
        public IntelDecoderValue Decoder { get; set; }
        public IntelDecoderOptions DecoderOptions { get; set; }

        IDecoderValue IDecodingSettings.Decoder => Decoder;
        IDecoderOptions IDecodingSettings.DecoderOptions => DecoderOptions;

        public static IntelDecodingSettings Default => new IntelDecodingSettings
        {
            Decoder = IntelDecoderValue.H264,
            DecoderOptions = IntelDecoderOptions.Default
        };
    }
}