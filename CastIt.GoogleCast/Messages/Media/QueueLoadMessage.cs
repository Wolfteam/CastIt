using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Extensions;
using CastIt.GoogleCast.Messages.Base;
using CastIt.GoogleCast.Models.Media;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CastIt.GoogleCast.Messages.Media
{
    internal class QueueLoadMessage : SessionMessage
    {
        public List<QueueItem> Items { get; set; }
            = new List<QueueItem>();

        [JsonIgnore]
        internal RepeatMode RepeatMode { get; set; }

        [JsonProperty(PropertyName = "repeatMode")]
        private string RepeatModeString
        {
            get { return RepeatMode.GetName(); }
            set { RepeatMode = value.Parse<RepeatMode>(); }
        }

        public int StartIndex { get; set; }
    }
}
