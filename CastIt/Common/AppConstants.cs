using System.Collections.Generic;
using System.Diagnostics;

namespace CastIt.Common
{
    public class AppConstants
    {
        public const double MinWindowWidth = 650;
        public const double MinWindowHeight = 208;

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

        public const int MaxCharsPerString = 1000;

        public static string AppName
            => "CastIt";

#if DEBUG
        public static string AppVersion
            => FileVersionInfo.GetVersionInfo(typeof(AppConstants).Assembly.Location).FileVersion + " - DEBUG";
#else
        public static string AppVersion
            => FileVersionInfo.GetVersionInfo(typeof(AppConstants).Assembly.Location).FileVersion;
#endif

        public static IReadOnlyList<string> AppAccentColors => new List<string>
        {
            AccentColorLightBlue, AccentColorLimeGreen, AccentColorPink,
            AccentColorDarkOrange, AccentColorVividRed, AccentColorDarkCyan,
            AccentColorDarkGreen, AccentColorDarkMagenta, AccentColorMagenta,
            AccentColorDarkGray, AccentColorOrange, AccentColorYellow,
            AccentColorDarkBlue, AccentColorViolet, AccentColorLightGrey
        };
    }
}
