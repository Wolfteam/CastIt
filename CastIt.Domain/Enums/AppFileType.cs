using System;

namespace CastIt.Domain.Enums
{
    [Flags]
    public enum AppFileType
    {
        Na = 1 << 0,
        Local = 1 << 1,
        Url = 1 << 2,
        Hls = 1 << 3,
        LocalVideo = 1 << 4,
        LocalMusic = 1 << 5,
        LocalSubtitle = 1 << 6
    }
}
