using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Enums;
using CastIt.Test.Models;

namespace CastIt.Test.Services
{
    public interface IServerAppSettingsService : IBaseAppSettingsService<ServerAppSettings>
    {
        string CurrentAppMigration { get; set; }
        bool StartFilesFromTheStart { get; set; }
        bool PlayNextFileAutomatically { get; set; }
        bool ForceVideoTranscode { get; set; }
        bool ForceAudioTranscode { get; set; }
        VideoScaleType VideoScale { get; set; }
        bool EnableHardwareAcceleration { get; set; }
        SubtitleFgColorType CurrentSubtitleFgColor { get; set; }
        SubtitleBgColorType CurrentSubtitleBgColor { get; set; }
        SubtitleFontScaleType CurrentSubtitleFontScale { get; set; }
        TextTrackFontStyleType CurrentSubtitleFontStyle { get; set; }
        TextTrackFontGenericFamilyType CurrentSubtitleFontFamily { get; set; }
        double SubtitleDelayInSeconds { get; set; }
        bool LoadFirstSubtitleFoundAutomatically { get; set; }
    }
}