using System.Collections.Generic;

namespace CastIt.Models.Youtube
{
    public class YoutubeMedia
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Url { get; set; }
        public int SelectedQuality { get; set; }
        public List<int> Qualities { get; set; }
            = new List<int>();
        public bool IsHls { get; set; }
    }
}
