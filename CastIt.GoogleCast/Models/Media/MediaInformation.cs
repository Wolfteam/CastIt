using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Shared.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Models.Media
{
    public class MediaInformation
    {
        public string ContentId { get; set; }

        [JsonIgnore]
        public StreamType StreamType { get; set; } = StreamType.Buffered;

        [JsonProperty(PropertyName = "streamType")]
        private string StreamTypeString
        {
            get { return StreamType.GetName(); }
            set { StreamType = value.Parse<StreamType>(); }
        }

        public string ContentType { get; set; }

        public GenericMediaMetadata Metadata { get; set; }

        public double? Duration { get; set; }

        public Dictionary<string, string> CustomData { get; set; }
            = new Dictionary<string, string>();

        public List<Track> Tracks { get; set; }
            = new List<Track>();

        public TextTrackStyle TextTrackStyle { get; set; }
    }
}
