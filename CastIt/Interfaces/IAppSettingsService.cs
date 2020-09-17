using CastIt.Common.Enums;
using CastIt.GoogleCast.Enums;

namespace CastIt.Interfaces
{
    public interface IAppSettingsService
    {
        AppLanguageType Language { get; set; }
        AppThemeType AppTheme { get; set; }
        string AccentColor { get; set; }
        string CurrentAppMigration { get; set; }
        double WindowWidth { get; set; }
        double WindowHeight { get; set; }
        bool IsPlayListExpanded { get; set; }
        bool ShowFileDetails { get; set; }
        bool StartFilesFromTheStart { get; set; }
        bool PlayNextFileAutomatically { get; set; }
        bool ForceVideoTranscode { get; set; }
        bool ForceAudioTranscode { get; set; }
        VideoScaleType VideoScale { get; set; }
        bool EnableHardwareAcceleration { get; set; }
        bool MinimizeToTray { get; set; }
        bool ShowPlayListTotalDuration { get; set; }

        SubtitleFgColorType CurrentSubtitleFgColor { get; set; }
        SubtitleBgColorType CurrentSubtitleBgColor { get; set; }
        SubtitleFontScaleType CurrentSubtitleFontScale { get; set; }
        TextTrackFontStyleType CurrentSubtitleFontStyle { get; set; }
        TextTrackFontGenericFamilyType CurrentSubtitleFontFamily { get; set; }
        double SubtitleDelayInSeconds { get; set; }
        bool LoadFirstSubtitleFoundAutomatically { get; set; }

        void SaveSettings();
    }
}
