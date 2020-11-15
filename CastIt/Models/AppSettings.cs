using CastIt.Domain.Enums;
using CastIt.GoogleCast.Enums;

namespace CastIt.Models
{
    public class AppSettings
    {
        public AppLanguageType Language { get; set; }
        public AppThemeType AppTheme { get; set; }
        public string AccentColor { get; set; }
        public string CurrentAppMigration { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool IsPlayListExpanded { get; set; }
        public bool ShowFileDetails { get; set; }
        public bool StartFilesFromTheStart { get; set; }
        public bool PlayNextFileAutomatically { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ShowPlayListTotalDuration { get; set; }
        public SubtitleFgColorType CurrentSubtitleFgColor { get; set; }
        public SubtitleBgColorType CurrentSubtitleBgColor { get; set; }
        public SubtitleFontScaleType CurrentSubtitleFontScale { get; set; }
        public TextTrackFontStyleType CurrentSubtitleFontStyle { get; set; }
        public TextTrackFontGenericFamilyType CurrentSubtitleFontFamily { get; set; }
        public double SubtitleDelayInSeconds { get; set; }
        public bool LoadFirstSubtitleFoundAutomatically { get; set; }
    }
}
