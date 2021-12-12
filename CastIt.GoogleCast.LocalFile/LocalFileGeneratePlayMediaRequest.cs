using CastIt.Domain.Enums;
using CastIt.GoogleCast.Models.Play;

namespace CastIt.GoogleCast.LocalFile
{
    public class LocalFileGeneratePlayMediaRequest : GeneratePlayMediaRequest
    {
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public int SubTitleStreamIndex { get; set; }
        public string ThumbnailUrl { get; set; }
        public HwAccelDeviceType HwAccel { get; set; }
        public VideoScaleType VideoScale { get; set; }
    }
}
