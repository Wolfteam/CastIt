using CastIt.Domain.Enums;

namespace CastIt.Domain.Dtos.Requests
{
    public class PlayAppFileRequestDto
    {
        public string Mrl { get; set; }
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public double Seconds { get; set; }

        public bool VideoNeedsTranscode { get; set; }
        public bool AudioNeedsTranscode { get; set; }
        public HwAccelDeviceType HwAccelToUse { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public int SelectedQuality { get; set; }
        public string VideoWidthAndHeight { get; set; }
    }
}
