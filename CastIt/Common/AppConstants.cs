using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CastIt.Common
{
    public class AppConstants
    {
        public const string AccentColorLightBlue = "#0077dd";
        public const string AccentColorLimeGreen = "#1be556";
        public const string AccentColorPink = "#ee0088";
        public const string AccentColorDarkOrange = "#cc4400";
        public const string AccentColorVividRed = "#ee1122";
        public const string AccentColorDarkCyan = "#008899";
        public const string AccentColorDarkGreen = "#118833";
        public const string AccentColorDarkMagenta = "#881199";
        public const string AccentColorMagenta = "#ff00e3";
        public const string AccentColorDarkGray = "#777777";
        public const string AccentColorOrange = "#f57c00";
        public const string AccentColorYellow = "#e2fd00";
        public const string AccentColorDarkBlue = "#0063b1";
        public const string AccentColorViolet = "#8600ff";
        public const string AccentColorLightGrey = "#a8a9aa";

        public const string FullElapsedTimeFormat = @"hh\:mm\:ss";
        public const string ShortElapsedTimeFormat = @"mm\:ss";

        public static IReadOnlyList<string> AllowedFormats
            => AllowedVideoFormats.Concat(AllowedMusicFormats).ToList();
        public static IReadOnlyList<string> AllowedVideoFormats => new List<string>
        {
            ".mp4",
            ".avi",
            ".mkv",
            ".m4v"
        };

        public static IReadOnlyList<string> AllowedMusicFormats => new List<string>
        {
            ".mp3",
            ".ogg",
            ".wav",
            ".aac",
            ".flac",
        };

        public static string AllowedFormatsString
            => string.Join(";", AllowedFormats.Select(ext => $"*{ext}"));

        public static string AppName
            => "CastIt";

        public string AppVersion
            => FileVersionInfo.GetVersionInfo(typeof(AppConstants).Assembly.Location).FileVersion;

        public static IReadOnlyList<string> AppAccentColors => new List<string>
        {
            AccentColorLightBlue, AccentColorLimeGreen, AccentColorPink,
            AccentColorDarkOrange, AccentColorVividRed, AccentColorDarkCyan,
            AccentColorDarkGreen, AccentColorDarkMagenta, AccentColorMagenta,
            AccentColorDarkGray, AccentColorOrange, AccentColorYellow,
            AccentColorDarkBlue, AccentColorViolet, AccentColorLightGrey
        };

        public static double MinWindowWidth = 650;
        public static double MinWindowHeight = 208;

        public const int ThumbnailsEachSeconds = 5;
    }
}
