using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.AMD
{
    public class AmdEncoderOptions : IEncoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public AmdEncoderOptions()
        {
        }

        public AmdEncoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        );

        public AmdEncoderOptions WithGPU(int index)
        {
            _settings.Add(new KeyValuePair<string, string>("-gpu", index.ToString()));
            return this;
        }

        public AmdEncoderOptions WithPreset(IPresetValue preset)
        {
            _settings.Add(new KeyValuePair<string, string>("-preset", preset.Value));
            return this;
        }

        public AmdEncoderOptions WithVsync(int value)
        {
            _settings.Add(new KeyValuePair<string, string>("-vsync", value.ToString()));
            return this;
        }

        public AmdEncoderOptions WithForcingAsIDRFrames()
        {
            _settings.Add(new KeyValuePair<string, string>("-forced-idr", "1"));
            return this;
        }

        public static AmdEncoderOptions Default =>
            new AmdEncoderOptions()
                .WithPreset(AmdEncoderPresetValue.HighPerformance)
                .WithVsync(0)
                .WithForcingAsIDRFrames();
    }
}