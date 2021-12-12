using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Enums;

namespace CastIt.Shared.Models
{
    public class ServerAppSettings
    {
        public string FFmpegExePath { get; set; }
        public string FFprobeExePath { get; set; }

        public string CurrentAppMigration { get; set; }
        public bool StartFilesFromTheStart { get; set; }
        public bool PlayNextFileAutomatically { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }

        public SubtitleFgColorType CurrentSubtitleFgColor { get; set; }
        public SubtitleBgColorType CurrentSubtitleBgColor { get; set; }
        public SubtitleFontScaleType CurrentSubtitleFontScale { get; set; }
        public TextTrackFontStyleType CurrentSubtitleFontStyle { get; set; }
        public TextTrackFontGenericFamilyType CurrentSubtitleFontFamily { get; set; }
        public double SubtitleDelayInSeconds { get; set; }
        public bool LoadFirstSubtitleFoundAutomatically { get; set; }

        public ServerAppSettings Copy()
        {
            return new ServerAppSettings
            {
                FFmpegExePath = FFmpegExePath,
                FFprobeExePath = FFprobeExePath,

                CurrentAppMigration = CurrentAppMigration,
                StartFilesFromTheStart = StartFilesFromTheStart,
                PlayNextFileAutomatically = PlayNextFileAutomatically,
                ForceVideoTranscode = ForceVideoTranscode,
                ForceAudioTranscode = ForceAudioTranscode,
                VideoScale = VideoScale,
                EnableHardwareAcceleration = EnableHardwareAcceleration,

                CurrentSubtitleFgColor = CurrentSubtitleFgColor,
                CurrentSubtitleBgColor = CurrentSubtitleBgColor,
                CurrentSubtitleFontScale = CurrentSubtitleFontScale,
                CurrentSubtitleFontStyle = CurrentSubtitleFontStyle,
                CurrentSubtitleFontFamily = CurrentSubtitleFontFamily,
                SubtitleDelayInSeconds = SubtitleDelayInSeconds,
                LoadFirstSubtitleFoundAutomatically = LoadFirstSubtitleFoundAutomatically
            };
        }

        public ServerAppSettings UpdateWith(ServerAppSettings other)
        {
            if (!string.IsNullOrWhiteSpace(other.FFmpegExePath))
                FFmpegExePath = other.FFmpegExePath;

            if (!string.IsNullOrWhiteSpace(other.FFprobeExePath))
                FFprobeExePath = other.FFprobeExePath;

            StartFilesFromTheStart = other.StartFilesFromTheStart;
            PlayNextFileAutomatically = other.PlayNextFileAutomatically;
            ForceVideoTranscode = other.ForceVideoTranscode;
            ForceAudioTranscode = other.ForceAudioTranscode;
            VideoScale = other.VideoScale;
            EnableHardwareAcceleration = other.EnableHardwareAcceleration;

            CurrentSubtitleFgColor = other.CurrentSubtitleFgColor;
            CurrentSubtitleBgColor = other.CurrentSubtitleBgColor;
            CurrentSubtitleFontScale = other.CurrentSubtitleFontScale;
            CurrentSubtitleFontStyle = other.CurrentSubtitleFontStyle;
            CurrentSubtitleFontFamily = other.CurrentSubtitleFontFamily;
            SubtitleDelayInSeconds = other.SubtitleDelayInSeconds;
            LoadFirstSubtitleFoundAutomatically = other.LoadFirstSubtitleFoundAutomatically;
            return this;
        }
    }
}
