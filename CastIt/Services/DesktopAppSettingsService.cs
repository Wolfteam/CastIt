using CastIt.Common;
using CastIt.Domain.Enums;
using CastIt.Domain.Utils;
using CastIt.Interfaces;
using CastIt.Models;
using CastIt.Shared.Settings;
using CastIt.Shared.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CastIt.Services
{
    internal class DesktopAppSettingsService : BaseAppSettingsService<AppSettings>, IDesktopAppSettingsService
    {
        protected override string BasePath
            => AppFileUtils.GetBaseAppFolder();

        public override string AppSettingsFilename
            => "DesktopAppSettings.json";

        #region Properties
        public AppLanguageType Language
        {
            get => Settings.Language;
            set => Settings.Language = value;
        }

        public AppThemeType AppTheme
        {
            get => Settings.AppTheme;
            set => Settings.AppTheme = value;
        }

        public string AccentColor
        {
            get => Settings.AccentColor;
            set => Settings.AccentColor = value;
        }

        public double WindowWidth
        {
            get => Settings.WindowWidth;
            set => Settings.WindowWidth = value;
        }

        public double WindowHeight
        {
            get => Settings.WindowHeight;
            set => Settings.WindowHeight = value;
        }

        public bool IsPlayListExpanded
        {
            get => Settings.IsPlayListExpanded;
            set => Settings.IsPlayListExpanded = value;
        }

        public bool ShowFileDetails
        {
            get => Settings.ShowFileDetails;
            set => Settings.ShowFileDetails = value;
        }

        public bool MinimizeToTray
        {
            get => Settings.MinimizeToTray;
            set => Settings.MinimizeToTray = value;
        }

        public bool ShowPlayListTotalDuration
        {
            get => Settings.ShowPlayListTotalDuration;
            set => Settings.ShowPlayListTotalDuration = value;
        }

        public bool UseGridViewForPlayLists
        {
            get => Settings.UseGridViewForPlayLists;
            set => Settings.UseGridViewForPlayLists = value;
        }

        public string ServerUrl
        {
            get => Settings.ServerUrl;
            set => Settings.ServerUrl = value;
        }
        #endregion

        public DesktopAppSettingsService(ILogger<DesktopAppSettingsService> logger, ITelemetryService telemetryService)
            : base(logger, telemetryService)
        {
        }

        public override Task<AppSettings> UpdateSettings(AppSettings settings, bool saveToFileSystem = false)
        {
            throw new NotImplementedException();
        }

        public override Task<AppSettings> GetDefaultAppSettings()
        {
            var settings = new AppSettings
            {
                AppTheme = AppThemeType.Dark,
                AccentColor = AppConstants.AccentColorVividRed,
                Language = AppLanguageType.English,
                IsPlayListExpanded = false,
                WindowHeight = AppConstants.MinWindowHeight,
                WindowWidth = AppConstants.MinWindowWidth,
                ShowFileDetails = true,
                PlayNextFileAutomatically = true,
                MinimizeToTray = true,
                ShowPlayListTotalDuration = true,
                UseGridViewForPlayLists = true
            };

            return Task.FromResult(settings);
        }
    }
}
