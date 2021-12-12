using System.Threading.Tasks;

namespace CastIt.Shared.Settings
{
    public interface IBaseAppSettingsService<T> where T : class, new()
    {
        T Settings { get; }
        string AppSettingsFilename { get; }

        Task Init();

        Task<T> SaveDefaultSettings();

        Task SaveCurrentSettings();

        Task SaveSettings(T settings);

        Task<T> UpdateSettings(T settings, bool saveToFileSystem = false);

        Task<T> LoadSettings();

        string GetAppSettingsPath();

        void DeleteAppSettings();

        bool AppSettingsExists();
    }
}
