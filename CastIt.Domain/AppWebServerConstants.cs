using System;

namespace CastIt.Domain
{
    public static class AppWebServerConstants
    {
        public const int NoStreamSelectedId = -1;
        public const int DefaultSelectedStreamId = 0;
        public const int DefaultQualitySelected = 360;

        public const string ChromeCastPlayPath = "Play";
        public const string ChromeCastImagesPath = "Images";
        public const string ChromeCastSubTitlesPath = "Subtitles";
        public const string ThumbnailPreviewImagesPath = "Images/Previews";
        public const int DefaultPort = 9696;
        public const string CastItHub = "CastItHub";

        public const string PortArgument = "--port";

        //The number of images in a tile
        public const int ThumbnailsPerImage = 25;

        //The number of images per row in a tile
        public const int ThumbnailsPerImageRow = 5;

        //The number of images per column in a tile
        public const int ThumbnailsPerImageColumn = 5;

        //The width of a thumbnail inside a tile
        public const double ThumbnailImageWidth = 350;

        //The height of a thumbnail inside a tile
        public const double ThumbnailImageHeight = 200;

        //Db limit on a string
        public const int MaxCharsPerString = 1000;

        //This represents how many seconds are in the generated images of a thumbnail
        public const int ThumbnailTileDuration = 25;

        public const string MissingFileText = "Missing";

        public static string ThumbnailWidthXHeightScale = $"{ThumbnailImageWidth}x{ThumbnailImageHeight}";

        public static string ThumbnailTileRowXColumn = $"{ThumbnailsPerImageRow}x{ThumbnailsPerImageColumn}";

        public static double ThumbnailTileTotalWidth = ThumbnailImageWidth * ThumbnailsPerImageColumn;

        public static double ThumbnailTileTotalHeight = ThumbnailImageHeight * ThumbnailsPerImageRow;

        public static bool InDocker
            => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    }
}
