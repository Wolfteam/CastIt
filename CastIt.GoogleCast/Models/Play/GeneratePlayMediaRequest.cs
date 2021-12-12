using CastIt.Domain.Enums;
using CastIt.Domain.Models.FFmpeg.Info;

namespace CastIt.GoogleCast.Models.Play
{
    public class GeneratePlayMediaRequest
    {
        public string Mrl { get; set; }
        public FFProbeFileInfo FileInfo { get; set; }
        public bool VideoNeedsTranscode { get; set; }
        public bool AudioNeedsTranscode { get; set; }
        public double Seconds { get; set; }
        public AppFileType FileType { get; set; }
    }
}
