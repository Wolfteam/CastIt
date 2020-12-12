using System.Collections.Generic;

namespace CastIt.Application.Server
{
    public static class AppWebServerConstants
    {
        public const string MediaPath = "/media";
        public const string ImagesPath = "/images";
        public const string SubTitlesPath = "/subtitles";
        public const int DefaultPort = 9696;

        public const string SecondsQueryParameter = "seconds";
        public const string FileQueryParameter = "file";
        public const string VideoStreamIndexParameter = "videoStream";
        public const string AudioStreamIndexParameter = "audioStream";
        public const string VideoNeedsTranscode = "videoNeedsTranscode";
        public const string AudioNeedsTranscode = "audioNeedsTranscode";
        public const string HwAccelTypeToUse = "hwAccelTypeToUse";
        public const string VideoWidthAndHeight = "videoWidthAndHeight";
        public const string VideoScaleParameter = "videoScale";

        public const string PortArgument = "--port";
        public const string FFmpegPathArgument = "--ffmpegBasePath";
        public const string FFprobePathArgument = "--ffprobeBasePath";

        public static IReadOnlyList<string> AllowedQueryParameters => new List<string>
        {
            SecondsQueryParameter,
            FileQueryParameter,
            VideoStreamIndexParameter,
            AudioStreamIndexParameter,
            VideoNeedsTranscode,
            AudioNeedsTranscode,
            HwAccelTypeToUse,
            VideoWidthAndHeight,
            VideoScaleParameter
        };
    }
}
