using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Models;

namespace CastIt.Interfaces
{
    public interface IDesktopAppSettingsService : IBaseAppSettingsService<DesktopAppSettings>
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
    }
}