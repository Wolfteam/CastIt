using CastIt.Domain.Enums;
using CastIt.Models;
using CastIt.Shared.Settings;

namespace CastIt.Interfaces
{
    public interface IDesktopAppSettingsService : IBaseAppSettingsService<AppSettings>
    {
        AppLanguageType Language { get; set; }

        AppThemeType AppTheme { get; set; }

        string AccentColor { get; set; }

        double WindowWidth { get; set; }

        double WindowHeight { get; set; }

        bool IsPlayListExpanded { get; set; }

        bool ShowFileDetails { get; set; }

        bool MinimizeToTray { get; set; }

        bool ShowPlayListTotalDuration { get; set; }

        bool UseGridViewForPlayLists { get; set; }

        string ServerUrl { get; set; }
    }
}