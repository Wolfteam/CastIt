﻿using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Shared.Models;
using CastIt.Shared.Settings;

namespace CastIt.Server.Interfaces
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
        string FFmpegExePath { get; set; }
        string FFprobeExePath { get; set; }
    }
}