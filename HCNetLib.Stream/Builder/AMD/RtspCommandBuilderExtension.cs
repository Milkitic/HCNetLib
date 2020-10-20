namespace HCNetLib.Stream.Builder.AMD
{
    public static class RtspCommandBuilderExtension
    {
        public static RtspCommandBuilder UseAmdEncodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            AmdEncodingSettings encodingSettings)
        {
            rtspCommandBuilder.WithEncodingSettings(encodingSettings);
            return rtspCommandBuilder;
        }
    }
}
