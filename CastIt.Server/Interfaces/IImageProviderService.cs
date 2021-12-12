using CastIt.Shared.Models;

namespace CastIt.Server.Interfaces
{
    public interface IImageProviderService
    {
        string GetPlayListImageUrl(ServerPlayList playList, ServerFileItem currentPlayedFile);

        string GetPlayListImageUrl(ServerPlayList playList);

        bool IsNoImage(string filename);

        string GetImagesPath();

        string GetNoImagePath();
    }
}