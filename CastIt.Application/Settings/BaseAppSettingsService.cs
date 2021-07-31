using CastIt.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CastIt.Application.Settings
{
    public abstract class BaseAppSettingsService<T> : IBaseAppSettingsService<T> where T : class, new()
    {
        #region Members
        protected readonly ILogger Logger;
        protected readonly ITelemetryService TelemetryService;

        public T Settings { get; protected set; }
        protected abstract string BasePath { get; }
        public abstract string AppSettingsFilename { get; }
        #endregion

        #region Constructors
        protected BaseAppSettingsService(ILogger logger, ITelemetryService telemetryService)
        {
            Logger = logger;
            TelemetryService = telemetryService;
        }
        #endregion

        #region Methods
        public virtual async Task Init()
        {
            Settings = await LoadSettings();
        }

        public virtual async Task<T> SaveDefaultSettings()
        {
            var defaultSettings = await GetDefaultAppSettings();
            await SaveSettings(defaultSettings);
            return Settings;
        }

        public virtual Task<T> LoadSettings()
        {
            try
            {
                if (!AppSettingsExists())
                {
                    Logger.LogInformation($"{nameof(LoadSettings)}: Settings does not exist. Creating a default one");
                    return SaveDefaultSettings();
                }
                string path = GetAppSettingsPath();
                var text = File.ReadAllText(path);
                var settings = File.Exists(path)
                    ? JsonConvert.DeserializeObject<T>(text)
                    : null;

                if (settings == null)
                    throw new NullReferenceException($"Settings couldn't be loaded from path = {path}");

                Logger.LogInformation($"{nameof(LoadSettings)}: Loaded settings = {JsonConvert.SerializeObject(settings)}");
                return Task.FromResult(settings);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(LoadSettings)}: Unknown error occurred while trying to retrieve user settings");
                TelemetryService.TrackError(ex);
                throw;
            }
        }

        public virtual Task SaveCurrentSettings()
            => SaveSettings(Settings);

        public virtual Task SaveSettings(T settings)
        {
            try
            {
                if (settings is null)
                    throw new ArgumentNullException(nameof(settings), "The user settings to be saved cannot be null. You need to initialize this service");

                string path = GetAppSettingsPath();
                string json = JsonConvert.SerializeObject(settings);
                Logger.LogInformation($"{nameof(SaveDefaultSettings)}: Trying to save settings = {json}");

                File.WriteAllText(path, json);

                Settings = settings;

                Logger.LogInformation($"{nameof(SaveDefaultSettings)}: Successfully saved settings");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SaveDefaultSettings)}: An unknown error occurred");
                TelemetryService.TrackError(ex);
                throw;
            }

            return Task.CompletedTask;
        }

        public abstract Task<T> UpdateSettings(T settings, bool saveToFileSystem = false);

        public string GetAppSettingsPath()
        {
            return Path.Combine(BasePath, AppSettingsFilename);
        }

        public void DeleteAppSettings()
        {
            if (!AppSettingsExists())
                return;

            string filepath = GetAppSettingsPath();
            File.Delete(filepath);
        }

        public bool AppSettingsExists()
        {
            string path = GetAppSettingsPath();
            bool exists = File.Exists(path);
            return exists;
        }

        public abstract Task<T> GetDefaultAppSettings();
        #endregion
    }
}
