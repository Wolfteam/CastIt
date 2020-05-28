using Newtonsoft.Json;

namespace CastIt.GoogleCast.Models
{
    public class Volume
    {
        public float? Level { get; set; }

        [JsonProperty(PropertyName = "muted")]
        public bool? IsMuted { get; set; }

        public float StepInterval { get; set; }
    }
}
