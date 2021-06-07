using CastIt.Application.Common;
using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using System;
using System.IO;
using System.Linq;

namespace CastIt.Application.FilePaths
{
    public class CommonFileService : ICommonFileService
    {
        public void DeleteFilesInDirectory(string dir, DateTime lastAccessTime)
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

        public void DeleteFilesInDirectory(string dir)
        {
            var files = new DirectoryInfo(dir)
                .GetFiles()
                .ToList();
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public bool IsLocalFile(string mrl)
        {
            return File.Exists(mrl);
        }

        public bool IsUrlFile(string mrl)
        {
            return Uri.TryCreate(mrl, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public bool IsVideoFile(string mrl)
        {
            return IsLocalFile(mrl) && IsVideoOrMusicFile(mrl, true);
        }

        public bool IsMusicFile(string mrl)
        {
            return IsLocalFile(mrl) && IsVideoOrMusicFile(mrl, false);
        }

        public bool IsHls(string mrl)
        {
            string ext = Path.GetExtension(mrl);
            return IsUrlFile(mrl) && FileFormatConstants.AllowedStreamingFormats.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public bool Exists(string mrl)
        {
            return IsLocalFile(mrl) || IsUrlFile(mrl);
        }

        public string GetFileName(string mrl)
        {
            return IsUrlFile(mrl) ? mrl : Path.GetFileName(mrl);
        }

        public string GetExtension(string mrl)
        {
            if (IsLocalFile(mrl))
                return Path.GetExtension(mrl).ToUpper();
            return IsUrlFile(mrl) ? "WEB" : "N/A";
        }

        public string GetFileSizeString(string mrl)
        {
            if (!IsLocalFile(mrl))
                return "N/A";
            var fileInfo = new FileInfo(mrl);
            return GetBytesReadable(fileInfo.Length);
        }

        public (string, string) TryGetSubTitlesLocalPath(string currentFilePath)
        {
            if (!IsLocalFile(currentFilePath))
            {
                return (null, null);
            }

            string filename = Path.GetFileNameWithoutExtension(currentFilePath);
            string dir = Path.GetDirectoryName(currentFilePath);

            var path = FileFormatConstants.AllowedSubtitleFormats
                .Select(format => Path.Combine(dir ?? string.Empty, filename + format))
                .FirstOrDefault(File.Exists);

            return (path, filename);
        }

        public (bool, string) IsSubtitle(string filePath)
        {
            if (!IsLocalFile(filePath))
                return (false, null);

            string filename = Path.GetFileName(filePath);
            return !FileFormatConstants.AllowedSubtitleFormats.Contains(Path.GetExtension(filePath).ToLower())
                ? (false, filename)
                : (true, filename);
        }

        public string CreateDirectory(string baseFolder, string folder)
        {
            return AppFileUtils.CreateDirectory(baseFolder, folder);
        }

        public string GetBytesReadable(long i)
        {
            // Get absolute value
            long absoluteI = i < 0 ? -i : i;
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absoluteI >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = i >> 50;
            }
            else if (absoluteI >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = i >> 40;
            }
            else if (absoluteI >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = i >> 30;
            }
            else if (absoluteI >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = i >> 20;
            }
            else if (absoluteI >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = i >> 10;
            }
            else if (absoluteI >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable /= 1024;
            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        public bool IsVideoOrMusicFile(string mrl, bool checkForVideo)
        {
            if (!IsLocalFile(mrl))
                return false;
            string ext = Path.GetExtension(mrl);
            return checkForVideo
                ? FileFormatConstants.AllowedVideoFormats.Contains(ext, StringComparer.OrdinalIgnoreCase)
                : FileFormatConstants.AllowedMusicFormats.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public AppFileType GetFileType(string mrl)
        {
            if (!Exists(mrl))
            {
                return AppFileType.Na;
            }

            bool isLocal = IsLocalFile(mrl);
            bool isUrl = IsUrlFile(mrl);
            bool isVideo = IsVideoFile(mrl);
            bool isMusic = IsMusicFile(mrl);
            (bool isSubtitle, _) = IsSubtitle(mrl);

            var value = AppFileType.Na;

            if (isLocal)
                value |= AppFileType.Local;

            if (isUrl)
                value |= AppFileType.Url;

            if (isVideo)
                value |= AppFileType.LocalVideo;

            if (isMusic)
                value |= AppFileType.LocalMusic;

            if (isSubtitle)
                value |= AppFileType.LocalSubtitle;

            if (value != AppFileType.Na)
                value &= ~AppFileType.Na;

            return value;
        }
    }
}
