using CastIt.Common;
using CastIt.Common.Enums;
using CastIt.Common.Utils;
using CastIt.Interfaces;
using CastIt.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace CastIt.Services
{
    public class AppSettingsService : IAppSettingsService
    {
        #region Members
        private AppSettings _appSettings;
        #endregion

        #region Properties
        public AppLanguageType Language
        {
            get => _appSettings.Language;
            set
            {
                _appSettings.Language = value;
                SaveSettings();
            }
        }

        public AppThemeType AppTheme
        {
            get => _appSettings.AppTheme;
            set
            {
                _appSettings.AppTheme = value;
                SaveSettings();
            }
        }

        public string AccentColor
        {
            get => _appSettings.AccentColor;
            set
            {
                _appSettings.AccentColor = value;
                SaveSettings();
            }
        }

        public string CurrentAppMigration
        {
            get => _appSettings.CurrentAppMigration;
            set
            {
                _appSettings.CurrentAppMigration = value;
                SaveSettings();
            }
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
        #endregion

        public AppSettingsService()
        {
            LoadSettings();
        }

        //public static AppSettings GetAppSettings()
        //{
        //    string basePath = FileUtils.GetBaseAppFolder();
        //    string filepath = Path.Combine(basePath, AppSettingsFilename);
        //    using var r = new StreamReader(filepath);
        //    string json = r.ReadToEnd();
        //    return JsonConvert.DeserializeObject<AppSettings>(json);
        //}

        #region Methods
        public void SaveSettings()
        {
            SaveSettings(_appSettings ?? new AppSettings()
            {
                AppTheme = AppThemeType.Dark,
                AccentColor = AppConstants.AccentColorVividRed,
                CurrentAppMigration = AppDbContext.CurrentAppMigration,
                Language = AppLanguageType.English,
                IsPlayListExpanded = true,
                WindowHeight = AppConstants.MinWindowHeight,
                WindowWidth = AppConstants.MinWindowWidth
            });
        }

        private void LoadSettings()
        {
            try
            {
                if (!FileUtils.AppSettingsExists())
                {
                    SaveSettings();
                    return;
                }
                string path = FileUtils.GetAppSettingsPath();
                var text = File.ReadAllText(path);
                var settings = File.Exists(path) ?
                    JsonConvert.DeserializeObject<AppSettings>(text) :
                    null;
                _appSettings = settings ?? new AppSettings();
            }
            catch (Exception e)
            {
                //_logger.LogError(e, $"{nameof(LoadSettings)}: Unknown error occurred while trying to retrieve user settings");
            }
        }

        private void SaveSettings(AppSettings settings)
        {
            //_logger.LogInformation($"{nameof(SaveSettings)}: Trying to save user settings...");
            try
            {
                if (settings is null)
                    throw new ArgumentNullException(nameof(settings), "The user settings to be saved cannot be null");

                string path = FileUtils.GetAppSettingsPath();
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(path, json);

                _appSettings = settings;

                //_logger.LogInformation($"{nameof(SaveSettings)}: Successfully saved user settings");
            }
            catch (Exception e)
            {
                //_logger.LogError(e, $"{nameof(SaveSettings)}: An unknown error occurred while trying to save the user settings");
            }
        }
        #endregion
    }
}
