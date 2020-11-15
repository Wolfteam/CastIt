using CastIt.Application.Common.Utils;
using System;
using System.IO;

namespace CastIt.Common.Utils
{
    public static class FileUtils
    {
        public static string GetBaseAppFolder()
        {
            var folder = CreateDirectory(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppConstants.AppName);
            return folder;
        }

        public static string CreateDirectory(string baseFolder, string folder)
        {
            return AppFileUtils.CreateDirectory(baseFolder, folder);
        }

        public static string GetLogsPath()
        {
            string basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, "Logs");
        }

        public static string GetDbConnectionString()
        {
            var path = Path.Combine(GetBaseAppFolder(), "CastIt.db");
            return $"Data Source={path}";
        }
    }
}