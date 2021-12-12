using CastIt.Domain.Models.FFmpeg.Info;
using CastIt.GoogleCast.Models.Media;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.GoogleCast.Models.Play
{
    public class PlayMediaRequest
    {
        public MediaInformation MediaInfo { get; set; }
        public bool Autoplay { get; set; } = true;
        public double SeekSeconds { get; set; }
        public List<int> ActiveTrackIds { get; set; } = new List<int>();
        public string OriginalMrl { get; set; }
        public bool IsHandledByServer { get; set; }
        public string ThumbnailUrl
            => MediaInfo.Metadata.Images.FirstOrDefault()?.Url;

        public string Base64 { get; set; }
        public bool NeedsTinyCode
            => !string.IsNullOrWhiteSpace(Base64);
        public FFProbeFileInfo FileInfo { get; set; }
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public int SubtitleStreamIndex { get; set; }
    }
}
