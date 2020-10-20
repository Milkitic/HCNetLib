using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultEncoderOptions : IEncoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public DefaultEncoderOptions()
        {
        }

        public DefaultEncoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        );

        public DefaultEncoderOptions WithPreset(IPresetValue preset)
        {
            _settings.Add(new KeyValuePair<string, string>("-preset", preset.Value));
            return this;
        }

        public static DefaultEncoderOptions Default =>
            new DefaultEncoderOptions()
                .WithPreset(DefaultEncoderPresetValue.VeryFast);
    }
}