using CastIt.Common.Enums;

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
    }
}
