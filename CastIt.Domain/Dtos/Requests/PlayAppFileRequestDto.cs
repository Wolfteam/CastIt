using CastIt.Domain.Enums;
using System.Collections.Generic;

namespace CastIt.Domain.Dtos.Requests
{
    public class PlayAppFileRequestDto
    {
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public List<string> StreamUrls { get; set; } = new List<string>();
        public double Seconds { get; set; }

        public bool VideoNeedsTranscode { get; set; }
        public bool AudioNeedsTranscode { get; set; }
        public HwAccelDeviceType HwAccelToUse { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public int SelectedQuality { get; set; }
        public string VideoWidthAndHeight { get; set; }

        public string ContentType { get; set; }
    }
}
