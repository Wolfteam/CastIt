using CastIt.Models;
using System;
using System.IO;

namespace CastIt.Common.Utils
{
    public class FileUtils
    {
        private const string AppSettingsFilename = "AppSettings.json";

        public static string GetDbConnectionString()
        {
            var fullPath = Path.Combine(GetBaseAppFolder(), AppDbContext.DatabaseName);
            return $"Filename={fullPath}";
        }

        public static string GetDbPath(string connectionString)
        {
            return connectionString.Replace("Filename=", string.Empty);
        }

        public static string GetBaseAppFolder()
        {
            var folder = CreateDirectory(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                AppConstants.AppName);
            return folder;
        }

        public static string GetLogsPath()
        {
            string basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, "Logs");
        }

        public static string GetPreviewsPath()
        {
            string basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, "Previews");
        }

        public static string GetThumbnailFilePath(string filename, int second)
        {
            return Path.Combine(GetPreviewsPath(), $"{filename}_{second}.jpg");
        }

        public static string CreateDirectory(string baseFolder, string folder)
        {
            var fullPath = Path.Combine(baseFolder, folder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        public static string GetAppSettingsPath()
        {
            string baseAppFolder = GetBaseAppFolder();
            return Path.Combine(baseAppFolder, AppSettingsFilename);
        }

        public static void DeleteAppSettings()
        {
            if (!AppSettingsExists())
                return;

            string basePath = GetBaseAppFolder();
            string filepath = Path.Combine(basePath, AppSettingsFilename);
            File.Delete(filepath);
        }

        public static bool AppSettingsExists()
        {
            string path = GetAppSettingsPath();
            bool exists = File.Exists(path);
            return exists;
        }
    }
}
