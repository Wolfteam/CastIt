using System.Threading.Tasks;

namespace CastIt.Shared.FilePaths
{
    public interface IFileService : ICommonFileService
    {
        string GetFFmpegFolder();

        string GetPreviewsPath();

        string GetFirstThumbnailFilePath(long id);

        string GetThumbnailFilePath(long id, long second);

        string GetPreviewThumbnailFilePath(long id);

        string GetSubTitleFolder();

        string GetSubTitleFilePath(string subsFilename = "subs.vtt");

        void DeleteAppLogsAndPreviews();

        void DeleteServerLogsAndPreviews(int maxDaysForPreviews = 3, int maxDaysForLogs = 3);

        string GetTemporalPreviewImagePath(long id);

        Task<string> DownloadAndSavePreviewImage(long id, string url, bool overrideIfExists = true);

        Task<string> DownloadAndSavePreviewImage(long id, string filename, string url, bool overrideIfExists = true);
    }
}
