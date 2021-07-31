﻿namespace CastIt.Domain.Dtos.Requests
{
    public class SetFileOptionsRequestDto
    {
        public int StreamIndex { get; set; }
        public bool IsAudio { get; set; }
        public bool IsSubTitle { get; set; }
        public bool IsQuality { get; set; }
    }
}
