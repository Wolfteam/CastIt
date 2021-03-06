﻿using CastIt.Domain.Enums;

namespace CastIt.Domain.Dtos.Responses
{
    public class AppSettingsResponseDto
    {
        public bool StartFilesFromTheStart { get; set; }
        public bool PlayNextFileAutomatically { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }
    }
}
