﻿using CastIt.Domain.Enums;

namespace CastIt.Domain.Dtos.Requests
{
    public class UpdateCliAppSettingsRequestDto
    {
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }
    }
}