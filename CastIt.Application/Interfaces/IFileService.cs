namespace CastIt.Application.Interfaces
{
    public interface IFileService : ICommonFileService
    {
        string GetFFmpegFolder();

        string GetFFmpegPath();

        string GetFFprobePath();

        string GetPreviewsPath();

        string GetFirstThumbnailFilePath(string filename);

        string GetThumbnailFilePath(string filename, long second);

        string GetPreviewThumbnailFilePath(string filename);

        string GetClosestThumbnail(string filePath, long tentativeSecond);

        string GetSubTitleFolder();

        string GetSubTitleFilePath(string subsFilename = "subs.vtt");

        void DeleteAppLogsAndPreviews();

        void DeleteServerLogsAndPreviews();
    }
}
