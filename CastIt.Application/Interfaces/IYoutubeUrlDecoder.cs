using CastIt.Domain.Models.Youtube;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Application.Interfaces
{
    public interface IYoutubeUrlDecoder
    {
        bool IsYoutubeUrl(string url);
        bool IsPlayListAndVideo(string url);
        bool IsPlayList(string url);
        Task<YoutubeMedia> Parse(string url, int? desiredQuality, bool getFinalUrl = true);
        Task<List<string>> ParseYouTubePlayList(string url, CancellationToken cancellationToken);
    }
}
