using CastIt.Domain.Enums;

namespace CastIt.Domain.Models.FFmpeg.Transcode
{
    public class TranscodeVideoFileBuilder : BaseTranscodeFileBuilder<TranscodeVideoFileBuilder, TranscodeVideoFile>
    {
        public TranscodeVideoFileBuilder WithStreams(int videoStreamIndex, int audioStreamIndex)
        {
            File.VideoStreamIndex = videoStreamIndex;
            File.AudioStreamIndex = audioStreamIndex;
            return this;
        }

        public TranscodeVideoFileBuilder WithDefaults(
            HwAccelDeviceType hwAccelDeviceType = HwAccelDeviceType.None,
            VideoScaleType scaleType = VideoScaleType.Original,
            int quality = -1,
            string withXHeight = null)
        {
            File.HwAccelDeviceType = hwAccelDeviceType;
            File.VideoScaleType = scaleType;
            File.Quality = quality;
            File.VideoWidthAndHeight = withXHeight;
            return this;
        }
    }

    public class TranscodeVideoFile : BaseTranscodeFile
    {
        public int VideoStreamIndex { get; set; }
        public int AudioStreamIndex { get; set; }
        public HwAccelDeviceType HwAccelDeviceType { get; set; }
        public VideoScaleType VideoScaleType { get; set; }
        public int Quality { get; set; }
        public string VideoWidthAndHeight { get; set; }
    }
}
