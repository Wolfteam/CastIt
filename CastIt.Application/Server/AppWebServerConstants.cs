namespace CastIt.Application.Server
{
    public static class AppWebServerConstants
    {
        public const int NoStreamSelectedId = -1;
        public const int DefaultSelectedStreamId = 0;
        public const int DefaultQualitySelected = 360;

        public const string ChromeCastPlayPath = "ChromeCastPlay";
        public const string ChromeCastImagesPath = "ChromeCastImages";
        public const string ChromeCastSubTitlesPath = "ChromeCastSubtitles";
        public const string ThumbnailPreviewImagesPath = "Images/Previews";
        public const int DefaultPort = 9696;

        public const string PortArgument = "--port";
        public const string FFmpegPathArgument = "--ffmpegBasePath";
        public const string FFprobePathArgument = "--ffprobeBasePath";
    }
}
