using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Youtube
{
    public interface IYoutubeUrlDecoder
    {
        bool IsYoutubeUrl(string url);

        bool IsPlayListAndVideo(string url);

        bool IsPlayList(string url);

        Task<BasicYoutubeMedia> ParseBasicInfo(
            string url,
            CancellationToken cancellationToken = default);

        Task<YoutubeMedia> Parse(
            string url,
            int? desiredQuality = null,
            CancellationToken cancellationToken = default);

        Task<YoutubeMedia> Parse(
            BasicYoutubeMedia basicInfo,
            int? desiredQuality = null,
            CancellationToken cancellationToken = default);

        Task<List<string>> ParsePlayList(
            string url,
            CancellationToken cancellationToken = default);
    }
}
