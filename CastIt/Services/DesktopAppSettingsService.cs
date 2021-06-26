using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Application.Settings;
using CastIt.Common;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Models;
using CastIt.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CastIt.Services
{
    internal class DesktopAppSettingsService : BaseAppSettingsService<DesktopAppSettings>, IDesktopAppSettingsService
    {
        protected override string BasePath
            => AppFileUtils.GetBaseAppFolder();

        public override string AppSettingsFilename
            => "AppSettings.json";

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
        #endregion

        public DesktopAppSettingsService(ILogger<DesktopAppSettingsService> logger, ITelemetryService telemetryService)
            : base(logger, telemetryService)
        {
        }

        public override Task<DesktopAppSettings> UpdateSettings(DesktopAppSettings settings, bool saveToFileSystem = false)
        {
            throw new NotImplementedException();
        }

        public override Task<DesktopAppSettings> GetDefaultAppSettings()
        {
            var settings = new DesktopAppSettings
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
