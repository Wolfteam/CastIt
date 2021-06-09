using System.Threading.Tasks;

namespace CastIt.Application.Interfaces
{
    public interface IBaseAppSettingsService<T> where T : class, new()
    {
        T Settings { get; }
        string AppSettingsFilename { get; }

        Task Init();

        Task<T> SaveDefaultSettings();

        Task SaveSettings(T settings);

        Task<T> LoadSettings();

        string GetAppSettingsPath();

        void DeleteAppSettings();

        bool AppSettingsExists();
    }
}
