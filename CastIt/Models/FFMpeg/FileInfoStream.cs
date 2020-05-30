using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Models.FFMpeg
{
    public class FileInfoStream
    {
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "codec_name")]
        public string CodecName { get; set; }

        [JsonProperty(PropertyName = "codec_type")]
        public string CodecType { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public string Profile { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "pix_fmt")]
        public string PixelFormat { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public FileInfoTag Tag { get; set; }

        public string WidthAndHeight
            => $"{Width}x{Height}";

        public bool IsVideo
            => CodecType == "video";

        public bool IsAudio
            => CodecType == "audio";

        public bool VideoCodecIsValid(IEnumerable<string> allowedCodecs)
            => IsVideo && allowedCodecs.Contains(CodecName, StringComparer.OrdinalIgnoreCase);

        public bool VideoContainerIsValid(string fileExtension, IEnumerable<string> allowedContainers)
            => IsVideo && allowedContainers.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

        public bool VideoProfileIsValid(IEnumerable<string> allowedProfiles)
            => IsVideo && allowedProfiles.Contains(Profile, StringComparer.OrdinalIgnoreCase);

        public bool VideoLevelIsValid(int maxLevelAllowed)
            => IsVideo && maxLevelAllowed >= Level;

        public bool AudioCodecIsValid(IEnumerable<string> allowedCodecs)
            => IsAudio && allowedCodecs.Contains(CodecName, StringComparer.OrdinalIgnoreCase);

        public bool AudioProfileIsValid(IEnumerable<string> allowedProfiles)
            => IsAudio && allowedProfiles.Contains(Profile, StringComparer.OrdinalIgnoreCase);
    }
}
