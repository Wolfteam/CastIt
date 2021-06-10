using CastIt.Application.Interfaces;
using CastIt.Application.Settings;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Enums;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    //TODO: SHOULD I SPLIT THE SETTINGS CLASS?
    internal class ServerAppSettingsService : BaseAppSettingsService<ServerAppSettings>, IServerAppSettingsService
    {
        protected override string BasePath
            => Directory.GetCurrentDirectory();

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
        #endregion

        public ServerAppSettingsService(
            ILogger<ServerAppSettingsService> logger,
            ITelemetryService telemetryService)
            : base(logger, telemetryService)
        {
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
                ForceAudioTranscode = true,
                ForceVideoTranscode = true,
                StartFilesFromTheStart = true,
                SubtitleDelayInSeconds = 0,
                CurrentSubtitleBgColor = SubtitleBgColorType.Transparent,
                //TODO THIS
                FFprobePath = "TODO",
                FFmpegPath = "TODO"
            };

            return Task.FromResult(defaultSettings);
        }
    }
}
