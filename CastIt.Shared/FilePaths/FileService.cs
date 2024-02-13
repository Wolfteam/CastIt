using CastIt.Domain.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CastIt.Shared.FilePaths
{
    public class FileService : CommonFileService, IFileService
    {
        private readonly string _generatedFilesFolderPath;

        public const string DefaultFFmpegFolder = "FFMpeg";
        public const string PreviewsFolderName = "Previews";
        public const string SubTitlesFolderName = "SubTitles";
        public const string TemporalImagePreviewFilename = "TEMP";

        public FileService()
            : this(AppFileUtils.GetBaseAppFolder())
        {
        }

        public FileService(string generatedFilesFolderPath)
        {
            _generatedFilesFolderPath = generatedFilesFolderPath ?? throw new ArgumentNullException(nameof(generatedFilesFolderPath));
        }


        public string GetFFmpegFolder()
        {
            var basePath = Path.Combine(AppFileUtils.GetBaseAppFolder(), DefaultFFmpegFolder);
            return CreateDirectory(basePath, string.Empty);
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

        public string GetFirstThumbnailFilePath(long id)
            => GetThumbnailFilePath(id, 0);

        public string GetThumbnailFilePath(long id, long second)
        {
            return Path.Combine(GetPreviewsPath(), $"{id}_{second:D2}.jpg");
        }

        public string GetPreviewThumbnailFilePath(long id)
        {
            return Path.Combine(GetPreviewsPath(), $"{id}_%02d.jpg");
        }

        public string GetTempPreviewThumbnailFilePath(long id)
        {
            return GetFirstThumbnailFilePath(id);
        }

        public string GetSubTitleFilePath(string subsFilename = "subs.vtt")
        {
            var basePath = GetSubTitleFolder();
            return Path.Combine(basePath, subsFilename);
        }

        public void DeleteAppLogsAndPreviews()
        {
            DeleteFilesInDirectory(AppFileUtils.GetDesktopLogsPath(), DateTime.Now.AddDays(-3));
        }

        public void DeleteServerLogsAndPreviews(int maxDaysForPreviews = 3, int maxDaysForLogs = 3)
        {
            DeleteFilesInDirectory(GetPreviewsPath(), DateTime.Now.AddDays(-maxDaysForPreviews));
            DeleteFilesInDirectory(AppFileUtils.GetServerLogsPath(), DateTime.Now.AddDays(-maxDaysForLogs));
        }

        public string GetTemporalPreviewImagePath(long id)
        {
            string path = GetTempPreviewThumbnailFilePath(id);
            return !Exists(path) ? null : path;
        }

        public Task<string> DownloadAndSavePreviewImage(long id, string url, bool overrideIfExists = true)
        {
            string filename = $"{id}_{TemporalImagePreviewFilename}";
            return DownloadAndSavePreviewImage(id, filename, url, overrideIfExists);
        }

        public async Task<string> DownloadAndSavePreviewImage(long id, string filename, string url, bool overrideIfExists = true)
        {
            string path = GetTempPreviewThumbnailFilePath(id);
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
