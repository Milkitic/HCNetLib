namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvEncoderPresetValue : IPresetValue
    {
        public NvEncoderPresetValue(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }
        public string Value { get; }

        public static NvEncoderPresetValue Default => new NvEncoderPresetValue(0, "default");
        public static NvEncoderPresetValue Slow => new NvEncoderPresetValue(1, "slow");
        public static NvEncoderPresetValue Medium => new NvEncoderPresetValue(2, "medium");
        public static NvEncoderPresetValue Fast => new NvEncoderPresetValue(3, "fast");
        public static NvEncoderPresetValue HighPerformance => new NvEncoderPresetValue(4, "hp");
        public static NvEncoderPresetValue HighQuality => new NvEncoderPresetValue(5, "hq");
        public static NvEncoderPresetValue BlurayDisk => new NvEncoderPresetValue(6, "bd");
        public static NvEncoderPresetValue LowLatency => new NvEncoderPresetValue(7, "ll");
        public static NvEncoderPresetValue LowLatencyHighQuality => new NvEncoderPresetValue(8, "llhq");
        public static NvEncoderPresetValue LowLatencyHighPerformance => new NvEncoderPresetValue(9, "llhp");
        public static NvEncoderPresetValue Lossless => new NvEncoderPresetValue(10, "lossless");
        public static NvEncoderPresetValue LosslessHighPerformance => new NvEncoderPresetValue(11, "losslesshp");
    }
}