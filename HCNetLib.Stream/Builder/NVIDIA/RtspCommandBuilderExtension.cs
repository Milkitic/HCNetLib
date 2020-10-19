namespace HCNetLib.Stream.Builder.NVIDIA
{
    public static class RtspCommandBuilderExtension
    {
        public static RtspCommandBuilder UseNvidiaDecodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            NvDecodingSettings decodingSettings)
        {
            rtspCommandBuilder.UseDecodingSettings(decodingSettings);
            return rtspCommandBuilder;
        }
        public static RtspCommandBuilder UseNvidiaEncodingSettings(this RtspCommandBuilder rtspCommandBuilder,
            NvEncodingSettings encodingSettings)
        {
            rtspCommandBuilder.WithEncodingSettings(encodingSettings);
            return rtspCommandBuilder;
        }
    }
}
