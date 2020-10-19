namespace HCNetLib.Stream.Builder.CPU
{
    public static class RtspCommandBuilderExtension
    {
        public static RtspCommandBuilder UseDefaultDecodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            DefaultDecodingSettings decodingSettings)
        {
            rtspCommandBuilder.UseDecodingSettings(decodingSettings);
            return rtspCommandBuilder;
        }
        public static RtspCommandBuilder UseDefaultEncodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            DefaultEncodingSettings encodingSettings)
        {
            rtspCommandBuilder.WithEncodingSettings(encodingSettings);
            return rtspCommandBuilder;
        }
    }
}
