using Newtonsoft.Json;
using System.Collections.Generic;

namespace CastIt.Models.FFMpeg
{
    public class FFProbeFileInfo
    {
        [JsonProperty(PropertyName = "streams")]
        public List<FileInfoStream> Streams { get; set; }
            = new List<FileInfoStream>();

        [JsonProperty(PropertyName = "format")]
        public FileInfoFormat Format { get; set; }
    }
}
