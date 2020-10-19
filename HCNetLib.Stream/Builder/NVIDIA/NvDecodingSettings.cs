namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvDecodingSettings : IDecodingSettings
    {
        public Manufacture Manufacture => Manufacture.NVIDIA;
        public NvDecoderValue Decoder { get; set; }
        public NvDecoderOptions DecoderOptions { get; set; }

        IDecoderValue IDecodingSettings.Decoder => Decoder;
        IDecoderOptions IDecodingSettings.DecoderOptions => DecoderOptions;

        public static NvDecodingSettings Default => new NvDecodingSettings
        {
            Decoder = NvDecoderValue.H264,
            DecoderOptions = NvDecoderOptions.Default
        };
    }
}