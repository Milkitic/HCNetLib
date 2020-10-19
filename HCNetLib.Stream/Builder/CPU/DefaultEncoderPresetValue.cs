namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultEncoderPresetValue : IPresetValue
    {
        public DefaultEncoderPresetValue(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }
        public string Value { get; }

        public static DefaultEncoderPresetValue UltraFast => new DefaultEncoderPresetValue(0, "ultrafast");
        public static DefaultEncoderPresetValue SuperFast => new DefaultEncoderPresetValue(1, "superfast");
        public static DefaultEncoderPresetValue VeryFast => new DefaultEncoderPresetValue(2, "veryfast");
        public static DefaultEncoderPresetValue Faster => new DefaultEncoderPresetValue(3, "faster");
        public static DefaultEncoderPresetValue Fast => new DefaultEncoderPresetValue(4, "fast");
        public static DefaultEncoderPresetValue Medium => new DefaultEncoderPresetValue(5, "medium");
        public static DefaultEncoderPresetValue Slow => new DefaultEncoderPresetValue(6, "slow");
        public static DefaultEncoderPresetValue Slower => new DefaultEncoderPresetValue(7, "slower");
        public static DefaultEncoderPresetValue VerySlow => new DefaultEncoderPresetValue(8, "veryslow");
    }
}