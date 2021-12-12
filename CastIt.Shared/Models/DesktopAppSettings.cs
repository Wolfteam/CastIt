using CastIt.Domain.Enums;

namespace CastIt.Shared.Models
{
    public class DesktopAppSettings
    {
        public AppLanguageType Language { get; set; }
        public AppThemeType AppTheme { get; set; }
        public string AccentColor { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public bool IsPlayListExpanded { get; set; }
        public bool ShowFileDetails { get; set; }
        public bool PlayNextFileAutomatically { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool ShowPlayListTotalDuration { get; set; }
        public bool UseGridViewForPlayLists { get; set; }
    }
}
