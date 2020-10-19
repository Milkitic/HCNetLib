using System.Collections.Generic;
using System.Linq;

namespace HCNetLib.Stream.Builder.NVIDIA
{
    public class NvDecoderOptions : IDecoderOptions
    {
        private List<KeyValuePair<string, string>> _settings = new List<KeyValuePair<string, string>>();

        public NvDecoderOptions()
        {
        }

        public NvDecoderOptions(string option, string value)
        {
            _settings.Add(new KeyValuePair<string, string>(option, value));
        }

        public string Value => " " + string.Join(" ",
            _settings.Select(k => $"{k.Key} {k.Value}")
        ) + " ";

        public NvDecoderOptions WithGPU(int index)
        {
            _settings.Add(new KeyValuePair<string, string>("-gpu", index.ToString()));
            return this;
        }

        public static NvDecoderOptions Default => new NvDecoderOptions();
    }
}