using CastIt.Domain.Enums;

namespace CastIt.Domain.Extensions
{
    public static class AppFileTypeExtensions
    {
        public static bool IsLocalOrHls(this AppFileType type)
            => type.HasFlag(AppFileType.Hls) || type.IsLocal();

        public static bool IsMusic(this AppFileType type)
            => type.HasFlag(AppFileType.LocalMusic);

        public static bool IsVideo(this AppFileType type)
            => type.HasFlag(AppFileType.LocalVideo);

        public static bool IsVideoOrMusic(this AppFileType type)
            => type.IsVideo() || type.IsMusic();

        public static bool IsSubtitle(this AppFileType type)
            => type.HasFlag(AppFileType.LocalSubtitle);

        public static bool IsUrl(this AppFileType type)
            => type.HasFlag(AppFileType.Url);

        public static bool DoesNotExist(this AppFileType type)
            => type.HasFlag(AppFileType.Na);

        public static bool IsLocal(this AppFileType type)
            => type.IsMusic() || type.IsVideo();
    }
}
