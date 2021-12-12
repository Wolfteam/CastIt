using System;
using System.Collections.Generic;
using System.Linq;

namespace CastIt.Domain
{
    public static class FileFormatConstants
    {
        public const double ThumbnailWidth = 200;
        public const double ThumbnailHeight = 150;
        public const string FullElapsedTimeFormat = @"hh\:mm\:ss";
        public const string ShortElapsedTimeFormat = @"mm\:ss";

        public static IReadOnlyList<string> AllowedFormats
            => AllowedVideoFormats.Concat(AllowedMusicFormats).ToList();

        public static IReadOnlyList<string> AllowedVideoFormats => new List<string>
        {
            ".mp4",
            ".avi",
            ".mkv",
            ".m4v"
        };

        public static IReadOnlyList<string> AllowedMusicFormats => new List<string>
        {
            ".mp3",
            ".ogg",
            ".wav",
            ".aac",
            ".flac",
            ".m4a",
            ".wav"
        };

        public static IReadOnlyList<string> AllowedSubtitleFormats => new List<string>
        {
            ".srt",
            ".ass",
            ".webvtt"
        };

        public static IReadOnlyList<string> AllowedStreamingFormats = new List<string>
        {
            ".m3u8"
        };

        public static string AllowedFormatsString
            => string.Join(";", AllowedFormats.Select(ext => $"*{ext}"));

        public static string AllowedSubtitleFormatsString
            => string.Join(";", AllowedSubtitleFormats.Select(ext => $"*{ext}"));

        public static string FormatDuration(double seconds)
        {
            //here backslash is used to tell that colon is
            //not the part of format, it just a character that we want in output
            var time = TimeSpan.FromSeconds(seconds);
            return time.ToString(time.Hours > 0 ? FullElapsedTimeFormat : ShortElapsedTimeFormat);
        }

        public static string FormatDuration(double playedSeconds, double totalSeconds, bool isUrlFile, bool exists)
        {
            string elapsed = FormatDuration(playedSeconds);
            string total = FormatDuration(totalSeconds);
            if (isUrlFile && totalSeconds <= 0)
                return $"{elapsed}";

            if (!exists)
            {
                return $"{elapsed} / {AppWebServerConstants.MissingFileText}";
            }

            return $"{elapsed} / {total}";
        }
    }
}
