﻿using CastIt.Common.Enums;

namespace CastIt.Server.Dtos.Requests
{
    public class AppSettingsRequestDto : BaseSocketRequestDto
    {
        public bool StartFilesFromTheStart { get; set; }
        public bool PlayNextFileAutomatically { get; set; }
        public bool ForceVideoTranscode { get; set; }
        public bool ForceAudioTranscode { get; set; }
        public VideoScaleType VideoScale { get; set; }
        public bool EnableHardwareAcceleration { get; set; }
    }
}