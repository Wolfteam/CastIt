using CastIt.Domain.Enums;

namespace CastIt.Domain.Extensions
{
    public static class AppFileTypeExtensions
    {
        public static bool IsLocalOrHls(this AppFileType type)
            => type.HasFlag(AppFileType.Hls) || type.IsLocal();

        public static bool IsLocalMusic(this AppFileType type)
            => type.HasFlag(AppFileType.LocalMusic);

        public static bool IsLocalVideo(this AppFileType type)
            => type.HasFlag(AppFileType.LocalVideo);

        public static bool IsVideoOrMusic(this AppFileType type)
            => type.IsLocalVideo() || type.IsLocalMusic();

        public static bool IsLocalSubtitle(this AppFileType type)
            => type.HasFlag(AppFileType.LocalSubtitle);

        public static bool IsUrl(this AppFileType type)
            => type.HasFlag(AppFileType.Url);

        public static bool DoesNotExist(this AppFileType type)
            => type.HasFlag(AppFileType.Na);

        public static bool IsLocal(this AppFileType type)
            => type.IsLocalMusic() || type.IsLocalVideo();
    }
}
