using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.CPU
{
    public class DefaultDecoderOptions : IDecoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public DefaultDecoderOptions()
        {
        }

        public DefaultDecoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => " " + string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        ) + " ";

        public static DefaultDecoderOptions Default => new DefaultDecoderOptions();
    }
}