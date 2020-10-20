namespace HCNetLib.Stream.Builder.AMD
{
    // currently no specialized decoder of amd is available
    internal class AmdDecodingSettings : IDecodingSettings
    {
        public Manufacture Manufacture => Manufacture.AMD;
        public AmdDecoderValue Decoder { get; set; }
        public AmdDecoderOptions DecoderOptions { get; set; }

        IDecoderValue IDecodingSettings.Decoder => Decoder;
        IDecoderOptions IDecodingSettings.DecoderOptions => DecoderOptions;
    }
}