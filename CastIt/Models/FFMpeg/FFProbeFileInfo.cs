using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Models.FFMpeg
{
    public class FFProbeFileInfo
    {
        [JsonProperty(PropertyName = "streams")]
        public List<FileInfoStream> Streams { get; set; }
            = new List<FileInfoStream>();

        [JsonProperty(PropertyName = "format")]
        public FileInfoFormat Format { get; set; }

        public List<FileInfoStream> Videos
            => Streams.Where(s => s.IsVideo).ToList();

        public List<FileInfoStream> Audios
            => Streams.Where(s => s.IsAudio).ToList();

        public List<FileInfoStream> SubTitles
            => Streams.Where(s => s.IsSubTitle).ToList();

        public string GetVideoResolution(int videoIndex = 0)
        {
            if (Videos.Count == 0)
                return string.Empty;

            var video = Videos[videoIndex];
            return video.WidthAndHeightText;
        }
    }
}
