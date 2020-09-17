using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.GoogleCast.Enums;
using CastIt.Interfaces;
using CastIt.Models;
using MvvmCross.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CastIt.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        #region Members
        private readonly IMvxLog _logger;
        private readonly ITelemetryService _telemetryService;
        private AppSettings _appSettings;
        #endregion

        #region Properties
        public AppLanguageType Language
        {
            get => _appSettings.Language;
            set => _appSettings.Language = value;
        }

        public AppThemeType AppTheme
        {
            get => _appSettings.AppTheme;
            set => _appSettings.AppTheme = value;
        }

        public string AccentColor
        {
            get => _appSettings.AccentColor;
            set => _appSettings.AccentColor = value;
        }

        public string CurrentAppMigration
        {
            get => _appSettings.CurrentAppMigration;
            set => _appSettings.CurrentAppMigration = value;
        }

        public double WindowWidth
        {
            get => _appSettings.WindowWidth;
            set => _appSettings.WindowWidth = value;
        }

        public double WindowHeight
        {
            get => _appSettings.WindowHeight;
            set => _appSettings.WindowHeight = value;
        }

        public bool IsPlayListExpanded
        {
            get => _appSettings.IsPlayListExpanded;
            set => _appSettings.IsPlayListExpanded = value;
        }

        public bool ShowFileDetails
        {
            get => _appSettings.ShowFileDetails;
            set => _appSettings.ShowFileDetails = value;
        }

        public bool StartFilesFromTheStart
        {
            get => _appSettings.StartFilesFromTheStart;
            set => _appSettings.StartFilesFromTheStart = value;
        }

        public bool PlayNextFileAutomatically
        {
            get => _appSettings.PlayNextFileAutomatically;
            set => _appSettings.PlayNextFileAutomatically = value;
        }

        public bool ForceVideoTranscode
        {
            get => _appSettings.ForceVideoTranscode;
            set => _appSettings.ForceVideoTranscode = value;
        }

        public bool ForceAudioTranscode
        {
            get => _appSettings.ForceAudioTranscode;
            set => _appSettings.ForceAudioTranscode = value;
        }

        public VideoScaleType VideoScale
        {
            get => _appSettings.VideoScale;
            set => _appSettings.VideoScale = value;
        }

        public bool EnableHardwareAcceleration
        {
            get => _appSettings.EnableHardwareAcceleration;
            set => _appSettings.EnableHardwareAcceleration = value;
        }

        public bool MinimizeToTray
        {
            get => _appSettings.MinimizeToTray;
            set => _appSettings.MinimizeToTray = value;
        }

        public bool ShowPlayListTotalDuration
        {
            get => _appSettings.ShowPlayListTotalDuration;
            set => _appSettings.ShowPlayListTotalDuration = value;
        }

        public SubtitleFgColorType CurrentSubtitleFgColor
        {
            get => _appSettings.CurrentSubtitleFgColor;
            set => _appSettings.CurrentSubtitleFgColor = value;
        }

        public SubtitleBgColorType CurrentSubtitleBgColor
        {
            get => _appSettings.CurrentSubtitleBgColor;
            set => _appSettings.CurrentSubtitleBgColor = value;
        }

        public SubtitleFontScaleType CurrentSubtitleFontScale
        {
            get => _appSettings.CurrentSubtitleFontScale;
            set => _appSettings.CurrentSubtitleFontScale = value;
        }

        public TextTrackFontStyleType CurrentSubtitleFontStyle
        {
            get => _appSettings.CurrentSubtitleFontStyle;
            set => _appSettings.CurrentSubtitleFontStyle = value;
        }

        public TextTrackFontGenericFamilyType CurrentSubtitleFontFamily
        {
            get => _appSettings.CurrentSubtitleFontFamily;
            set => _appSettings.CurrentSubtitleFontFamily = value;
        }

        public double SubtitleDelayInSeconds
        {
            get => _appSettings.SubtitleDelayInSeconds;
            set => _appSettings.SubtitleDelayInSeconds = value;
        }

        public bool LoadFirstSubtitleFoundAutomatically
        {
            get => _appSettings.LoadFirstSubtitleFoundAutomatically;
            set => _appSettings.LoadFirstSubtitleFoundAutomatically = value;
        }
        #endregion

        public AppSettingsService(IMvxLogProvider logProvider, ITelemetryService telemetryService)
        {
            _logger = logProvider.GetLogFor<AppSettingsService>();
            _telemetryService = telemetryService;
            LoadSettings();
        }

        #region Methods
        public void SaveSettings()
        {
            SaveSettings(_appSettings ?? new AppSettings
            {
                AppTheme = AppThemeType.Dark,
                AccentColor = AppConstants.AccentColorVividRed,
                CurrentAppMigration = null,
                Language = AppLanguageType.English,
                IsPlayListExpanded = false,
                WindowHeight = AppConstants.MinWindowHeight,
                WindowWidth = AppConstants.MinWindowWidth,
                EnableHardwareAcceleration = true,
                ShowFileDetails = true,
                PlayNextFileAutomatically = true,
                MinimizeToTray = true,
                VideoScale = VideoScaleType.Original,
                ShowPlayListTotalDuration = true,
                CurrentSubtitleFgColor = SubtitleFgColorType.White,
                CurrentSubtitleFontFamily = TextTrackFontGenericFamilyType.Casual,
                CurrentSubtitleFontStyle = TextTrackFontStyleType.Bold,
                CurrentSubtitleFontScale = SubtitleFontScaleType.HundredAndFifty,
                LoadFirstSubtitleFoundAutomatically = true
            });
        }

        private void LoadSettings()
        {
            try
            {
                if (!FileUtils.AppSettingsExists())
                {
                    _logger.Info($"{nameof(LoadSettings)}: Settings does not exist. Creating a default one");
                    SaveSettings();
                    return;
                }
                string path = FileUtils.GetAppSettingsPath();
                var text = File.ReadAllText(path);
                var settings = File.Exists(path) ?
                    JsonConvert.DeserializeObject<AppSettings>(text) :
                    null;

                if (settings != null)
                    _logger.Info($"{nameof(LoadSettings)}: Loaded settings = {JsonConvert.SerializeObject(settings)}");

                _appSettings = settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(LoadSettings)}: Unknown error occurred while trying to retrieve user settings");
                _telemetryService.TrackError(ex);
            }
        }

        private void SaveSettings(AppSettings settings)
        {
            try
            {
                if (settings is null)
                    throw new ArgumentNullException(nameof(settings), "The user settings to be saved cannot be null");

                string path = FileUtils.GetAppSettingsPath();
                string json = JsonConvert.SerializeObject(settings);
                _logger.Info($"{nameof(SaveSettings)}: Trying to save settings = {json}");

                File.WriteAllText(path, json);

                _appSettings = settings;

                _logger.Info($"{nameof(SaveSettings)}: Successfully saved settings");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{nameof(SaveSettings)}: An unknown error occurred");
                _telemetryService.TrackError(ex);
            }
        }
        #endregion
    }
}
