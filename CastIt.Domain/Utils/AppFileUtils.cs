using System;
using System.IO;

namespace CastIt.Domain.Utils;

public static class AppFileUtils
{
    public static string GetBaseAppFolder(string appName = "CastIt")
    {
        var specialFolder = OperatingSystem.IsWindows() || AppWebServerConstants.InDocker
            ? Environment.SpecialFolder.CommonApplicationData
            : Environment.SpecialFolder.LocalApplicationData;

        string basePath = Environment.GetFolderPath(specialFolder);

        string folder = CreateDirectory(basePath, appName);
        return folder;
    }

    private static string GetLogsPath(string logsFolder)
    {
        string basePath = GetBaseAppFolder();
        return CreateDirectory(basePath, logsFolder);
    }

    public static string GetDesktopLogsPath()
        => GetLogsPath("DesktopLogs");

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