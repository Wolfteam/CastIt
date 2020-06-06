﻿using System;
using System.IO;
using System.Linq;

namespace CastIt.Common.Utils
{
    public static class FileUtils
    {
        private const string AppSettingsFilename = "AppSettings.json";

        public static string GetFFMpegFolder()
        {
            var basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, "FFMpeg");
        }

        public static string GetFFMpegPath()
        {
            var basePath = GetFFMpegFolder();
            return Path.Combine(basePath, "ffmpeg.exe");
        }

        public static string GetFFprobePath()
        {
            var basePath = GetFFMpegFolder();
            return Path.Combine(basePath, "ffprobe.exe");
        }

        public static string GetDbConnectionString()
        {
            return Path.Combine(GetBaseAppFolder(), "CastIt.db");
        }

        public static string GetDbPath(string connectionString)
        {
            return connectionString.Replace("Filename=", string.Empty);
        }

        public static string GetBaseAppFolder()
        {
            var folder = CreateDirectory(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
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

        public static string GetThumbnailFilePath(string filename, long second)
        {
            return Path.Combine(GetPreviewsPath(), $"{filename}_{second:D2}.jpg");
        }

        public static string GetPreviewThumbnailFilePath(string filename)
        {
            return Path.Combine(GetPreviewsPath(), $"{filename}_%02d.jpg");
        }

        public static string GetClosestThumbnail(string filePath, long tentativeSecond)
        {
            long second = tentativeSecond / AppConstants.ThumbnailsEachSeconds;
            string folder = GetPreviewsPath();
            string filename = Path.GetFileName(filePath);
            string ext = Path.GetExtension(filePath);
            string searchPattern = $"{filename}_*";

            try
            {
                var files = Directory.EnumerateFiles(folder, searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".jpg"))
                    .Select(p => p.Substring(p.IndexOf(filename)).Replace(filename, string.Empty))
                    .Select(p => p.Substring(p.LastIndexOf("_") + 1, p.IndexOf(".") - 1))
                    .Select(p => long.Parse(p))
                    .ToList();

                if (!files.Any())
                    return null;

                long closest = files.Aggregate((x, y) => Math.Abs(x - second) < Math.Abs(y - second) ? x : y);

                if (Math.Abs(closest - second) > AppConstants.ThumbnailsEachSeconds)
                    return null;
                string previewPath = GetThumbnailFilePath(filename, closest);
                return previewPath;
            }
            catch (Exception)
            {
                return null;
            }
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

        public static void DeleteFilesInDirectory(string dir, DateTime lastAccessTime)
        {
            var files = new DirectoryInfo(dir)
                .GetFiles()
                .Where(f => f.LastAccessTime < lastAccessTime)
                .ToList();
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public static bool IsLocalFile(string mrl)
        {
            return File.Exists(mrl);
        }

        public static bool IsUrlFile(string mrl)
        {
            return Uri.TryCreate(mrl, UriKind.Absolute, out Uri uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsVideoFile(string mrl)
        {
            if (!IsLocalFile(mrl))
                return false;
            return IsVideoOrMusicFile(mrl, true);
        }

        public static bool IsMusicFile(string mrl)
        {
            if (!IsLocalFile(mrl))
                return false;
            return IsVideoOrMusicFile(mrl, false);
        }

        public static string GetSubTitleFolder()
        {
            var basePath = GetBaseAppFolder();
            return CreateDirectory(basePath, "SubTitles");
        }

        public static string GetSubTitleFilePath(string subsFilename = "subs.vtt")
        {
            var basePath = GetSubTitleFolder();
            return Path.Combine(basePath, subsFilename);
        }

        private static bool IsVideoOrMusicFile(string mrl, bool checkForVideo)
        {
            string ext = Path.GetExtension(mrl);
            if (checkForVideo)
                return AppConstants.AllowedVideoFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase);
            return AppConstants.AllowedMusicFormats.Contains(ext.ToLower(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
