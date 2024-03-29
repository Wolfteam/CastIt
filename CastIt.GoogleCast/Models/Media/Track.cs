﻿using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Shared.Enums;
using Newtonsoft.Json;

namespace CastIt.GoogleCast.Models.Media
{
    public class Track
    {
        public int TrackId { get; set; }

        [JsonIgnore]
        public TrackType Type { get; set; }

        [JsonProperty(PropertyName = "type")]
        private string TypeString
        {
            get { return Type.GetName(); }
            set { Type = value.Parse<TrackType>(); }
        }

        public string TrackContentType { get; set; } = "text/vtt";

        public string TrackContentId { get; set; }

        [JsonIgnore]
        public TextTrackType? SubType { get; set; }

        [JsonProperty(PropertyName = "subType")]
        private string SubTypeString
        {
            get => SubType.GetName();
            set => SubType = value.Parse<TextTrackType>();
        }

        public string Name { get; set; }

        public string Language { get; set; }
    }
}
