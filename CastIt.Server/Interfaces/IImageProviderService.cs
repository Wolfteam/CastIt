using CastIt.Shared.Models;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface IImageProviderService
    {
        byte[] NoImageBytes { get; }

        byte[] TransparentImageBytes { get; }

        Task Init();

        string GetPlayListImageUrl(ServerPlayList playList, ServerFileItem currentPlayedFile);

        string GetPlayListImageUrl(ServerPlayList playList);

        bool IsNoImage(string filename);

        string GetImagesPath();

        string GetNoImagePath();
    }
}