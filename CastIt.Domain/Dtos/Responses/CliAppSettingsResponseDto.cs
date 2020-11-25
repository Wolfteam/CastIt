using CastIt.Domain.Enums;

namespace CastIt.Domain.Dtos.Responses
{
    public class CliAppSettingsResponseDto
    {
        public string FFmpegBasePath { get; set; }
        public string FFprobeBasePath { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }
    }
}
