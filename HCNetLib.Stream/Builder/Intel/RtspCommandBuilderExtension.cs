namespace HCNetLib.Stream.Builder.Intel
{
    public static class RtspCommandBuilderExtension
    {
        public static RtspCommandBuilder UseIntelDecodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            IntelDecodingSettings decodingSettings)
        {
            rtspCommandBuilder.UseDecodingSettings(decodingSettings);
            return rtspCommandBuilder;
        }
        public static RtspCommandBuilder UseIntelEncodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            IntelEncodingSettings encodingSettings)
        {
            rtspCommandBuilder.WithEncodingSettings(encodingSettings);
            return rtspCommandBuilder;
        }
    }
}
