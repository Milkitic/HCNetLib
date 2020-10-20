using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.Intel
{
    public class IntelDecoderOptions : IDecoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public IntelDecoderOptions()
        {
        }

        public IntelDecoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        );

        public IntelDecoderOptions WithGPU(int index)
        {
            _settings.Add(new KeyValuePair<string, string>("-gpu", index.ToString()));
            return this;
        }

        public static IntelDecoderOptions Default => new IntelDecoderOptions();
    }
}