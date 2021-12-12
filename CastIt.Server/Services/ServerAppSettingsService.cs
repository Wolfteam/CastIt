using CastIt.Domain.Enums;
using CastIt.Domain.Utils;
using CastIt.FFmpeg;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Server.Interfaces;
using CastIt.Shared.Models;
using CastIt.Shared.Settings;
using CastIt.Shared.Telemetry;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    internal class ServerAppSettingsService : BaseAppSettingsService<ServerAppSettings>, IServerAppSettingsService
    {
        private readonly IServerService _serverService;
        private readonly IFFmpegService _fFmpegService;

        protected override string BasePath
            => AppFileUtils.GetBaseAppFolder();

        public override string AppSettingsFilename
            => "ServerAppSettings.json";

        #region Properties
        public string CurrentAppMigration
        {
            get => Settings.CurrentAppMigration;
            set => Settings.CurrentAppMigration = value;
        }

        public bool StartFilesFromTheStart
        {
            get => Settings.StartFilesFromTheStart;
            set => Settings.StartFilesFromTheStart = value;
        }

        public bool PlayNextFileAutomatically
        {
            get => Settings.PlayNextFileAutomatically;
            set => Settings.PlayNextFileAutomatically = value;
        }

        public bool ForceVideoTranscode
        {
            get => Settings.ForceVideoTranscode;
            set => Settings.ForceVideoTranscode = value;
        }

        public bool ForceAudioTranscode
        {
            get => Settings.ForceAudioTranscode;
            set => Settings.ForceAudioTranscode = value;
        }

        public VideoScaleType VideoScale
        {
            get => Settings.VideoScale;
            set => Settings.VideoScale = value;
        }

        public bool EnableHardwareAcceleration
        {
            get => Settings.EnableHardwareAcceleration;
            set => Settings.EnableHardwareAcceleration = value;
        }

        public SubtitleFgColorType CurrentSubtitleFgColor
        {
            get => Settings.CurrentSubtitleFgColor;
            set => Settings.CurrentSubtitleFgColor = value;
        }

        public SubtitleBgColorType CurrentSubtitleBgColor
        {
            get => Settings.CurrentSubtitleBgColor;
            set => Settings.CurrentSubtitleBgColor = value;
        }

        public SubtitleFontScaleType CurrentSubtitleFontScale
        {
            get => Settings.CurrentSubtitleFontScale;
            set => Settings.CurrentSubtitleFontScale = value;
        }

        public TextTrackFontStyleType CurrentSubtitleFontStyle
        {
            get => Settings.CurrentSubtitleFontStyle;
            set => Settings.CurrentSubtitleFontStyle = value;
        }

        public TextTrackFontGenericFamilyType CurrentSubtitleFontFamily
        {
            get => Settings.CurrentSubtitleFontFamily;
            set => Settings.CurrentSubtitleFontFamily = value;
        }

        public double SubtitleDelayInSeconds
        {
            get => Settings.SubtitleDelayInSeconds;
            set => Settings.SubtitleDelayInSeconds = value;
        }

        public bool LoadFirstSubtitleFoundAutomatically
        {
            get => Settings.LoadFirstSubtitleFoundAutomatically;
            set => Settings.LoadFirstSubtitleFoundAutomatically = value;
        }

        public string FFmpegExePath
        {
            get => Settings.FFmpegExePath;
            set => Settings.FFmpegExePath = value;
        }

        public string FFprobeExePath
        {
            get => Settings.FFprobeExePath;
            set => Settings.FFprobeExePath = value;
        }
        #endregion

        public ServerAppSettingsService(
            ILogger<ServerAppSettingsService> logger,
            ITelemetryService telemetryService,
            IServerService serverService,
            IFFmpegService fFmpegService)
            : base(logger, telemetryService)
        {
            _serverService = serverService;
            _fFmpegService = fFmpegService;
        }

        public override async Task<ServerAppSettings> UpdateSettings(ServerAppSettings settings, bool saveToFileSystem = false)
        {
            Logger.LogInformation($"{nameof(UpdateSettings)}: Trying to update settings...");
            _fFmpegService.RefreshFfmpegPath(settings.FFmpegExePath, settings.FFprobeExePath);
            Settings.UpdateWith(settings);
            if (saveToFileSystem)
            {
                Logger.LogInformation($"{nameof(UpdateSettings)}: Saving the updated settings to disk...");
                await SaveCurrentSettings();
            }
            _serverService.OnSettingsChanged?.Invoke(Settings);
            Logger.LogInformation($"{nameof(UpdateSettings)}: Settings were successfully updated");
            return Settings;
        }

        public override Task<ServerAppSettings> GetDefaultAppSettings()
        {
            var defaultSettings = new ServerAppSettings
            {
                CurrentAppMigration = null,
                EnableHardwareAcceleration = true,
                PlayNextFileAutomatically = true,
                VideoScale = VideoScaleType.Original,
                CurrentSubtitleFgColor = SubtitleFgColorType.White,
                CurrentSubtitleFontFamily = TextTrackFontGenericFamilyType.Casual,
                CurrentSubtitleFontStyle = TextTrackFontStyleType.Bold,
                CurrentSubtitleFontScale = SubtitleFontScaleType.HundredAndFifty,
                LoadFirstSubtitleFoundAutomatically = true,
                ForceAudioTranscode = false,
                ForceVideoTranscode = false,
                StartFilesFromTheStart = true,
                SubtitleDelayInSeconds = 0,
                CurrentSubtitleBgColor = SubtitleBgColorType.Transparent
            };

            return Task.FromResult(defaultSettings);
        }
    }
}
