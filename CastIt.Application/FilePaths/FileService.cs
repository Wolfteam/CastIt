using CastIt.Application.Common.Utils;
using CastIt.Application.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CastIt.Application.FilePaths
{
    public class FileService : CommonFileService, IFileService
    {
        private readonly string _ffmpegBasePath;
        private readonly string _ffprobeBasePath;
        private readonly string _generatedFilesFolderPath;
        private readonly int _thumbnailsEachSeconds;

        public const string DefaultFFmpegFolder = "FFMpeg";
        public const string DefaultFFmpegExecutableName = "ffmpeg.exe";
        public const string DefaultFFprobeExecutableName = "ffprobe.exe";
        public const string PreviewsFolderName = "Previews";
        public const string SubTitlesFolderName = "SubTitles";
        public const string TemporalImagePreviewFilename = "TEMP";

        public FileService(string generatedFilesFolderPath, int thumbnailsEachSeconds = 5)
        {
            _generatedFilesFolderPath = generatedFilesFolderPath ?? throw new ArgumentNullException(nameof(generatedFilesFolderPath));
            var dir = CreateDirectory(generatedFilesFolderPath, DefaultFFmpegFolder);
            _ffmpegBasePath = Path.Combine(dir, DefaultFFmpegExecutableName);
            _ffprobeBasePath = Path.Combine(dir, DefaultFFprobeExecutableName);
            _thumbnailsEachSeconds = thumbnailsEachSeconds <= 0
                ? throw new ArgumentOutOfRangeException(nameof(thumbnailsEachSeconds))
                : thumbnailsEachSeconds;
        }

        public FileService(string ffmpegBasePath, string ffprobeBasePath, string generatedFilesFolderPath, int thumbnailsEachSeconds = 5)
        {
            _ffmpegBasePath = ffmpegBasePath ?? throw new ArgumentNullException(nameof(ffmpegBasePath));
            _ffprobeBasePath = ffprobeBasePath ?? throw new ArgumentNullException(nameof(ffprobeBasePath));
            _generatedFilesFolderPath = generatedFilesFolderPath ?? throw new ArgumentNullException(nameof(generatedFilesFolderPath));
            _thumbnailsEachSeconds = thumbnailsEachSeconds <= 0
                ? throw new ArgumentOutOfRangeException(nameof(thumbnailsEachSeconds))
                : thumbnailsEachSeconds;

            if (!IsLocalFile(_ffmpegBasePath))
                throw new ArgumentException($"Path = {_ffmpegBasePath} does not exist");

            if (!IsLocalFile(_ffprobeBasePath))
                throw new ArgumentException($"Path = {_ffprobeBasePath} does not exist");
        }

        public string GetFFmpegFolder()
        {
            var basePath = Path.GetDirectoryName(_ffmpegBasePath);
            return CreateDirectory(basePath, string.Empty);
        }

        public string GetFFmpegPath()
        {
            return _ffmpegBasePath;
        }

        public string GetFFprobePath()
        {
            return _ffprobeBasePath;
        }

        public string GetPreviewsPath()
        {
            string basePath = _generatedFilesFolderPath;
            return CreateDirectory(basePath, PreviewsFolderName);
        }

        public string GetSubTitleFolder()
        {
            var basePath = _generatedFilesFolderPath;
            return CreateDirectory(basePath, SubTitlesFolderName);
        }

        public string GetFirstThumbnailFilePath(string filename)
            => GetThumbnailFilePath(filename, 0);

        public string GetThumbnailFilePath(string filename, long second)
        {
            return Path.Combine(GetPreviewsPath(), $"{filename}_{second:D2}.jpg");
        }

        public string GetPreviewThumbnailFilePath(string filename)
        {
            return Path.Combine(GetPreviewsPath(), $"{filename}_%02d.jpg");
        }

        public string GetClosestThumbnail(string filePath, long tentativeSecond)
        {
            long second = tentativeSecond / _thumbnailsEachSeconds;
            string folder = GetPreviewsPath();
            string filename = Path.GetFileName(filePath);
            string searchPattern = $"{filename}_*";

            try
            {
                var files = Directory.EnumerateFiles(folder, searchPattern, SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".jpg"))
                    .Select(p => p.Substring(p.IndexOf(filename, StringComparison.OrdinalIgnoreCase)).Replace(filename, string.Empty))
                    .Select(p => p.Substring(p.LastIndexOf("_", StringComparison.OrdinalIgnoreCase) + 1, p.IndexOf(".", StringComparison.OrdinalIgnoreCase) - 1))
                    .Select(long.Parse)
                    .ToList();

                if (!files.Any())
                    return null;

                long closest = files.Aggregate((x, y) => Math.Abs(x - second) < Math.Abs(y - second) ? x : y);

                if (Math.Abs(closest - second) > _thumbnailsEachSeconds)
                    return null;
                string previewPath = GetThumbnailFilePath(filename, closest);
                return previewPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetSubTitleFilePath(string subsFilename = "subs.vtt")
        {
            var basePath = GetSubTitleFolder();
            return Path.Combine(basePath, subsFilename);
        }

        public void DeleteAppLogsAndPreviews()
        {
            DeleteFilesInDirectory(GetPreviewsPath(), DateTime.Now.AddDays(-1));
            DeleteFilesInDirectory(AppFileUtils.GetLogsPath(), DateTime.Now.AddDays(-3));
        }

        public void DeleteServerLogsAndPreviews()
        {
            DeleteFilesInDirectory(GetPreviewsPath(), DateTime.Now.AddDays(-1));
            DeleteFilesInDirectory(AppFileUtils.GetServerLogsPath(), DateTime.Now.AddDays(-3));
        }

        public string GetTemporalPreviewImagePath(long id)
        {
            var filename = $"{id}_{TemporalImagePreviewFilename}";
            var path = GetPreviewThumbnailFilePath(filename);
            return !Exists(path) ? null : path;
        }

        public Task<string> DownloadAndSavePreviewImage(long id, string url, bool overrideIfExists = true)
        {
            var filename = $"{id}_{TemporalImagePreviewFilename}";
            return DownloadAndSavePreviewImage(filename, url, overrideIfExists);
        }

        public async Task<string> DownloadAndSavePreviewImage(string filename, string url, bool overrideIfExists = true)
        {
            var path = GetPreviewThumbnailFilePath(filename);
            if (Exists(path) && !overrideIfExists)
            {
                return path;
            }

            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsByteArrayAsync();
                if (Exists(path))
                {
                    File.Delete(path);
                }

                await File.WriteAllBytesAsync(path, content);
            }
            catch (Exception)
            {
                return null;
            }
            return path;
        }
    }
}
