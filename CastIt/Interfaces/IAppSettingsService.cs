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

        void SaveSettings();
    }
}
