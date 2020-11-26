using System;
using System.IO;

namespace CastIt.Application.Common.Utils
{
    public static class AppFileUtils
    {
        public static string GetBaseAppFolder(string appName = "CastIt")
        {
            var folder = CreateDirectory(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);
            return folder;
        }

        public static string GetLogsPath(string logsFolder = "Logs")
        {
            string basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, logsFolder);
        }

        public static string GetServerLogsPath()
            => GetLogsPath("ServerLogs");

        public static string GetDbConnectionString()
        {
            var path = Path.Combine(GetBaseAppFolder(), "CastIt.db");
            return $"Data Source={path}";
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
    }
}
