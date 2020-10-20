using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.AMD
{
    // currently no specialized decoder of amd is available
    internal class AmdDecoderOptions : IDecoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public AmdDecoderOptions()
        {
        }

        public AmdDecoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        );
    }
}