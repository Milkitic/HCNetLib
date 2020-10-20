namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelEncoderPresetValue : IPresetValue
    {
        public IntelEncoderPresetValue(int index, string value)
        {
            Index = index;
            Value = value;
        }

        public int Index { get; }
        public string Value { get; }

        public static IntelEncoderPresetValue VeryFast => new IntelEncoderPresetValue(2, "veryfast");
        public static IntelEncoderPresetValue Faster => new IntelEncoderPresetValue(3, "faster");
        public static IntelEncoderPresetValue Fast => new IntelEncoderPresetValue(4, "fast");
        public static IntelEncoderPresetValue Medium => new IntelEncoderPresetValue(5, "medium");
        public static IntelEncoderPresetValue Slow => new IntelEncoderPresetValue(6, "slow");
        public static IntelEncoderPresetValue Slower => new IntelEncoderPresetValue(7, "slower");
        public static IntelEncoderPresetValue VerySlow => new IntelEncoderPresetValue(8, "veryslow");
    }
}