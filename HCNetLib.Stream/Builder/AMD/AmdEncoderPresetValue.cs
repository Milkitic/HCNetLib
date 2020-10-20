namespace HCNetLib.Stream.Builder.AMD
{
    public class AmdEncoderPresetValue : IPresetValue
    {
        public AmdEncoderPresetValue(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }
        public string Value { get; }

        public static AmdEncoderPresetValue Default => new AmdEncoderPresetValue(0, "default");
        public static AmdEncoderPresetValue Slow => new AmdEncoderPresetValue(1, "slow");
        public static AmdEncoderPresetValue Medium => new AmdEncoderPresetValue(2, "medium");
        public static AmdEncoderPresetValue Fast => new AmdEncoderPresetValue(3, "fast");
        public static AmdEncoderPresetValue HighPerformance => new AmdEncoderPresetValue(4, "hp");
        public static AmdEncoderPresetValue HighQuality => new AmdEncoderPresetValue(5, "hq");
        public static AmdEncoderPresetValue BlurayDisk => new AmdEncoderPresetValue(6, "bd");
        public static AmdEncoderPresetValue LowLatency => new AmdEncoderPresetValue(7, "ll");
        public static AmdEncoderPresetValue LowLatencyHighQuality => new AmdEncoderPresetValue(8, "llhq");
        public static AmdEncoderPresetValue LowLatencyHighPerformance => new AmdEncoderPresetValue(9, "llhp");
        public static AmdEncoderPresetValue Lossless => new AmdEncoderPresetValue(10, "lossless");
        public static AmdEncoderPresetValue LosslessHighPerformance => new AmdEncoderPresetValue(11, "losslesshp");
    }
}