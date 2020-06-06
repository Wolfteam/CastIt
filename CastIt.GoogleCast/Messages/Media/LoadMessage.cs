using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Models.Media;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Messages.Media
{
    internal class LoadMessage : SessionMessage
    {
        public MediaInformation Media { get; set; }

        [JsonProperty(PropertyName = "autoplay")]
        public bool AutoPlay { get; set; }

        public List<int> ActiveTrackIds { get; set; }
            = new List<int>();
    }
}
