using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CastIt.Domain
{
    public static class FileFormatConstants
    {
        public const double ThumbnailWidth = 200;
        public const double ThumbnailHeight = 150;

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
            var time = TimeSpan.FromSeconds(seconds);
            string format = @"mm\:ss";
            if (time.TotalHours >= 1)
            {
                format = @"hh\:" + format;
            }

            if (time.TotalDays >= 1)
            {
                format = @"d\+" + format;
            }

            return time.ToString(format, CultureInfo.InvariantCulture);
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
