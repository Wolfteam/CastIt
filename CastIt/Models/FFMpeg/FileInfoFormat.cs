using Newtonsoft.Json;

namespace CastIt.Models.FFMpeg
{
    public class FileInfoFormat
    {
        [JsonProperty(PropertyName = "duration")]
        public double Duration { get; set; }

        [JsonProperty(PropertyName = "size")]
        public long Size { get; set; }

        [JsonProperty(PropertyName = "bit_rate")]
        public long BitRate { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public FileInfoTag Tag { get; set; }
    }
}
