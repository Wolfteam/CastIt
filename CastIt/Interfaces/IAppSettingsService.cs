using CastIt.Common.Enums;

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

        void SaveSettings();
    }
}
