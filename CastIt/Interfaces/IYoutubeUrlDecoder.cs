using CastIt.Models.Youtube;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IYoutubeUrlDecoder
    {
        bool IsYoutubeUrl(string url);
        bool IsPlayListAndVideo(string url);
        bool IsPlayList(string url);
        Task<YoutubeMedia> Parse(string url, int quality);
        Task<List<string>> ParseYouTubePlayList(string url);
    }
}
