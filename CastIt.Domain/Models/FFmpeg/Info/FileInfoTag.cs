﻿using Newtonsoft.Json;

namespace CastIt.Domain.Models.FFmpeg.Info
{
    public class FileInfoTag
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "artist")]
        public string Artist { get; set; }

        [JsonProperty(PropertyName = "album")]
        public string Album { get; set; }
    }
}
