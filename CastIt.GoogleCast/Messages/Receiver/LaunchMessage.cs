using CastIt.GoogleCast.Messages.Base;
using Newtonsoft.Json;

namespace CastIt.GoogleCast.Messages
{
    internal class LaunchMessage : MessageWithId
    {
        [JsonProperty(PropertyName = "appId")]
        public string ApplicationId { get; set; }
    }
}
