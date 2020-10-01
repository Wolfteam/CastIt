using System;
using System.IO;
using System.Linq;
using System.Text;

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
            string searchPattern = $"{filename}_*";

            try
            {
                var files = Directory.EnumerateFiles(folder, searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".jpg"))
                    .Select(p => p.Substring(p.IndexOf(filename)).Replace(filename, string.Empty))
                    .Select(p => p.Substring(p.LastIndexOf("_") + 1, p.IndexOf(".") - 1))
                    .Select(long.Parse)
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

        public static void DeleteFilesInDirectory(string dir)
        {
            var files = new DirectoryInfo(dir)
                .GetFiles()
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
            return IsLocalFile(mrl) && IsVideoOrMusicFile(mrl, true);
        }

        public static bool IsMusicFile(string mrl)
        {
            return IsLocalFile(mrl) && IsVideoOrMusicFile(mrl, false);
        }

        public static bool IsHls(string mrl)
        {
            string ext = Path.GetExtension(mrl);
            return IsUrlFile(mrl) && AppConstants.AllowedStreamingFormats.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static bool Exists(string mrl)
        {
            return IsLocalFile(mrl) || IsUrlFile(mrl);
        }

        public static string GetFileName(string mrl)
        {
            return IsUrlFile(mrl) ? mrl : Path.GetFileName(mrl);
        }

        public static string GetExtension(string mrl)
        {
            if (IsLocalFile(mrl))
                return Path.GetExtension(mrl).ToUpper();
            if (IsUrlFile(mrl))
                return "WEB";
            return "N/A";
        }

        public static string GetFileSizeString(string mrl)
        {
            if (!IsLocalFile(mrl))
                return "N/A";
            var fileInfo = new FileInfo(mrl);
            return GetBytesReadable(fileInfo.Length);
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

        public static Encoding GetEncoding(string filePath)
        {
            var encodingByBOM = GetEncodingByBOM(filePath);
            if (encodingByBOM != null)
                return encodingByBOM;

            // BOM not found :(, so try to parse characters into several encodings
            var encodingByParsingUTF8 = GetEncodingByParsing(filePath, Encoding.UTF8);
            if (encodingByParsingUTF8 != null)
                return encodingByParsingUTF8;

            var encodingByParsingLatin1 = GetEncodingByParsing(filePath, Encoding.GetEncoding("iso-8859-1"));
            if (encodingByParsingLatin1 != null)
                return encodingByParsingLatin1;

            var encodingByParsingUTF7 = GetEncodingByParsing(filePath, Encoding.UTF7);
            if (encodingByParsingUTF7 != null)
                return encodingByParsingUTF7;

            return null;   // no encoding found
        }

        private static Encoding GetEncodingByBOM(string filePath)
        {
            // Read the BOM
            var byteOrderMark = new byte[4];
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                file.Read(byteOrderMark, 0, 4);
            }

            // Analyze the BOM
            if (byteOrderMark[0] == 0x2b && byteOrderMark[1] == 0x2f && byteOrderMark[2] == 0x76) return Encoding.UTF7;
            if (byteOrderMark[0] == 0xef && byteOrderMark[1] == 0xbb && byteOrderMark[2] == 0xbf) return Encoding.UTF8;
            if (byteOrderMark[0] == 0xff && byteOrderMark[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (byteOrderMark[0] == 0xfe && byteOrderMark[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (byteOrderMark[0] == 0 && byteOrderMark[1] == 0 && byteOrderMark[2] == 0xfe && byteOrderMark[3] == 0xff) return Encoding.UTF32;

            return null;    // no BOM found
        }

        private static Encoding GetEncodingByParsing(string filePath, Encoding encoding)
        {
            var encodingVerifier = Encoding.GetEncoding(encoding.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());

            try
            {
                using (var textReader = new StreamReader(filePath, encodingVerifier, detectEncodingFromByteOrderMarks: true))
                {
                    while (!textReader.EndOfStream)
                    {
                        textReader.ReadLine();   // in order to increment the stream position
                    }

                    // all text parsed ok
                    return textReader.CurrentEncoding;
                }
            }
            catch (Exception ex) { }

            return null;    // 
        }

        private static bool IsVideoOrMusicFile(string mrl, bool checkForVideo)
        {
            string ext = Path.GetExtension(mrl);
            return checkForVideo
                ? AppConstants.AllowedVideoFormats.Contains(ext, StringComparer.OrdinalIgnoreCase)
                : AppConstants.AllowedMusicFormats.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            long absolute_i = i < 0 ? -i : i;
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = i >> 50;
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = i >> 40;
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = i >> 30;
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = i >> 20;
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = i >> 10;
            }
            else if (absolute_i >= 0x400) // Kilobyte
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
    }
}
