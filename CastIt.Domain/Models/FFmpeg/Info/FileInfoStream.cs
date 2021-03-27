using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Domain.Models.FFmpeg.Info
{
    public class FileInfoStream
    {
        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "codec_name")]
        public string CodecName { get; set; }

        [JsonProperty(PropertyName = "codec_type")]
        public string CodecType { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public string Profile { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "pix_fmt")]
        public string PixelFormat { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "sample_rate")]
        public long SampleRate { get; set; }

        [JsonProperty(PropertyName = "nb_frames")]
        public long NumberOfFrames { get; set; }

        [JsonProperty(PropertyName = "avg_frame_rate")]
        public string AverageFrameRate { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public FileInfoTag Tag { get; set; }

        //The CodecType in an audio file may return video if the stream is a png img
        public bool IsVideo
            => CodecType == "video" && Level != 0 && (NumberOfFrames > 1 || AverageFrameRate != "0/0");

        //Live videos does not have neither number of frames nor avg frame rate, that's why I use this one instead
        public bool IsHlsVideo
            => CodecType == "video" && Level != 0;

        public bool IsAudio
            => CodecType == "audio";

        public bool IsSubTitle
            => CodecType == "subtitle";

        public string WidthAndHeightText
            => $"{Width}x{Height}";

        private string TitleTag
            => string.IsNullOrEmpty(Tag?.Title) ? string.Empty : Tag?.Title;

        private string LanguageTag
            => $"{(string.IsNullOrEmpty(Tag?.Language) ? string.Empty : $"[{Tag.Language}]")}";

        public string VideoText
            => $"{CodecName} {Profile} {Level}, {PixelFormat}, {WidthAndHeightText}".Trim();

        public string AudioText
            => $"{TitleTag} {LanguageTag} ({CodecName} {Profile}, {SampleRate} Hz)".Trim();

        public string SubTitleText
            => $"{TitleTag} {LanguageTag} ({CodecName})".Trim();

        public bool VideoCodecIsValid(IEnumerable<string> allowedCodecs)
            => IsVideo && allowedCodecs.Contains(CodecName, StringComparer.OrdinalIgnoreCase);

        public bool VideoContainerIsValid(string fileExtension, IEnumerable<string> allowedContainers)
            => IsVideo && allowedContainers.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

        public bool VideoProfileIsValid(IEnumerable<string> allowedProfiles)
            => IsVideo && allowedProfiles.Contains(Profile, StringComparer.OrdinalIgnoreCase);

        public bool VideoLevelIsValid(int maxLevelAllowed)
            => IsVideo && maxLevelAllowed >= Level;

        public bool AudioCodecIsValid(IEnumerable<string> allowedCodecs)
            => IsAudio && allowedCodecs.Contains(CodecName, StringComparer.OrdinalIgnoreCase);

        public bool AudioProfileIsValid(IEnumerable<string> allowedProfiles)
            => IsAudio && allowedProfiles.Contains(Profile, StringComparer.OrdinalIgnoreCase);
    }
}
