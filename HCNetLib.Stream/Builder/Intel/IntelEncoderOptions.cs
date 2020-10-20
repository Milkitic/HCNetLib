using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelEncoderOptions : IEncoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public IntelEncoderOptions()
        {
        }

        public IntelEncoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        );

        public IntelEncoderOptions WithPreset(IPresetValue preset)
        {
            _settings.Add(new KeyValuePair<string, string>("-preset", preset.Value));
            return this;
        }

        public IntelEncoderOptions WithForcingAsIDRFrames()
        {
            _settings.Add(new KeyValuePair<string, string>("-forced_idr", "1"));
            return this;
        }

        public static IntelEncoderOptions Default =>
            new IntelEncoderOptions()
                .WithPreset(IntelEncoderPresetValue.Slower)
                .WithForcingAsIDRFrames();
    }
}