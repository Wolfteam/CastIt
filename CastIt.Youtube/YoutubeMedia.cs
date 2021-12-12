using System.Collections.Generic;
using System.Linq;

namespace CastIt.Youtube
{
    public class BasicYoutubeMedia
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public bool IsHls { get; set; }
        public string Body { get; set; }
        public VideoQualities VideoQualities { get; set; }
    }

    public class YoutubeMedia : BasicYoutubeMedia
    {
        public int SelectedQuality
            => VideoQualities.SelectedQuality;
        public List<int> Qualities
        {
            get
            {
                if (IsFromAdaptiveFormat || VideoQualities.UseAdaptiveFormats)
                {
                    return VideoQualities.FromAdaptiveFormats
                        .Where(q => q.ContainsOnlyVideo)
                        .Select(q => q.Quality)
                        .ToList();
                }

                return VideoQualities.FromFormats.ConvertAll(q => q.Quality);
            }
        }

        public string VideoUrl { get; private set; }
        public string AudioUrl { get; private set; }
        public bool IsFromAdaptiveFormat { get; private set; }
        public List<string> AdaptiveFormatUrls => new List<string>
        {
            VideoUrl,
            AudioUrl
        };

        public YoutubeMedia(BasicYoutubeMedia basicInfo)
        {
            IsHls = basicInfo.IsHls;
            Title = basicInfo.Title;
            Description = basicInfo.Description;
            ThumbnailUrl = basicInfo.ThumbnailUrl;
            Body = basicInfo.Body;
            VideoQualities = basicInfo.VideoQualities;
            Url = basicInfo.Url;
        }

        public void SetFromAdaptiveFormats(string videoUrl, string audioUrl)
        {
            VideoUrl = videoUrl;
            AudioUrl = audioUrl;
            IsFromAdaptiveFormat = true;
        }

        public void SetFromFormat(string url)
        {
            VideoUrl = url;
        }
    }
}
