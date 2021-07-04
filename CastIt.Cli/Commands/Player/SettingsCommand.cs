using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Enums;
using CastIt.GoogleCast.Shared.Enums;
using CastIt.Infrastructure.Models;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "settings", Description = "Gets or updates the server's settings", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class SettingsCommand : BaseCommand
    {
        //General
        [Option(CommandOptionType.SingleOrNoValue, Description = "The ffmpeg exe path", LongName = "ffmpeg", ShortName = "ffmpeg")]
        public string FFmpegPath { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The ffprobe exe path", LongName = "ffprobe", ShortName = "ffprobe")]
        public string FFprobePath { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if files should start from the start or if they should start where they were left", LongName = "play-from-start", ShortName = "play-from-start")]
        public bool? StartFilesFromTheStart { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if the next file in the playlist should be played automatically", LongName = "play-next-file", ShortName = "play-next-file")]
        public bool? PlayNextFileAutomatically { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if video transcode must be done for all files", LongName = "video-transcode", ShortName = "video-transcode")]
        public bool? ForceVideoTranscode { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if video transcode must be done for all files", LongName = "audio-transcode", ShortName = "audio-transcode")]
        public bool? ForceAudioTranscode { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the video scale for transcoded videos", LongName = "video-scale", ShortName = "video-scale")]
        public VideoScaleType? VideoScale { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if the hardware acceleration should be enabled for transcoded processes", LongName = "use-hw-accel", ShortName = "use-hw-accel")]
        public bool? EnableHardwareAcceleration { get; set; }

        //Subs specific
        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the foreground color of the subtitles", LongName = "subs-fg-color", ShortName = "subs-fg-color")]
        public SubtitleFgColorType? SubtitleFgColor { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the background color of the subtitles", LongName = "subs-bg-color", ShortName = "subs-bg-color")]
        public SubtitleBgColorType? SubtitleBgColor { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the font scale of the subtitles", LongName = "subs-font-scale", ShortName = "subs-font-scale")]
        public SubtitleFontScaleType? SubtitleFontScale { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the font style of the subtitles", LongName = "subs-font-style", ShortName = "subs-font-style")]
        public TextTrackFontStyleType? SubtitleFontStyle { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the font family of the subtitles", LongName = "subs-font-family", ShortName = "subs-font-family")]
        public TextTrackFontGenericFamilyType? SubtitleFontFamily { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets the delay in seconds of the subtitles", LongName = "subs-delay", ShortName = "subs-delay")]
        public double? SubtitleDelayInSeconds { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "Sets if first found subtitles should be loaded", LongName = "subs-auto-load", ShortName = "subs-auto-load")]
        public bool? LoadFirstSubtitleFoundAutomatically { get; set; }

        public SettingsCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            var patch = BuildPathDocument();

            if (!patch.Operations.Any())
            {
                AppConsole.WriteLine("Retrieving server settings...");
                var response = await CastItApi.GetCurrentSettings();
                CheckServerResponse(response);

                PrettyPrintAsJson(response.Result);
            }
            else
            {
                AppConsole.WriteLine("Updating server settings...");
                var response = await CastItApi.UpdateSettings(patch);
                CheckServerResponse(response);
            }

            return SuccessCode;
        }

        private JsonPatchDocument<ServerAppSettings> BuildPathDocument()
        {
            var patch = new JsonPatchDocument<ServerAppSettings>();

            if (!string.IsNullOrWhiteSpace(FFmpegPath))
                patch.Replace(p => p.FFmpegExePath, FFmpegPath);

            if (!string.IsNullOrWhiteSpace(FFprobePath))
                patch.Replace(p => p.FFprobeExePath, FFprobePath);

            if (StartFilesFromTheStart.HasValue)
                patch.Replace(p => p.StartFilesFromTheStart, StartFilesFromTheStart.Value);

            if (PlayNextFileAutomatically.HasValue)
                patch.Replace(p => p.PlayNextFileAutomatically, PlayNextFileAutomatically.Value);

            if (ForceVideoTranscode.HasValue)
                patch.Replace(p => p.ForceVideoTranscode, ForceVideoTranscode.Value);

            if (ForceAudioTranscode.HasValue)
                patch.Replace(p => p.ForceAudioTranscode, ForceAudioTranscode.Value);

            if (VideoScale.HasValue)
                patch.Replace(p => p.VideoScale, VideoScale.Value);

            if (EnableHardwareAcceleration.HasValue)
                patch.Replace(p => p.EnableHardwareAcceleration, EnableHardwareAcceleration.Value);

            if (SubtitleBgColor.HasValue)
                patch.Replace(p => p.CurrentSubtitleBgColor, SubtitleBgColor.Value);

            if (SubtitleFgColor.HasValue)
                patch.Replace(p => p.CurrentSubtitleFgColor, SubtitleFgColor.Value);

            if (SubtitleFontScale.HasValue)
                patch.Replace(p => p.CurrentSubtitleFontScale, SubtitleFontScale.Value);

            if (SubtitleFontStyle.HasValue)
                patch.Replace(p => p.CurrentSubtitleFontStyle, SubtitleFontStyle.Value);

            if (SubtitleFontFamily.HasValue)
                patch.Replace(p => p.CurrentSubtitleFontFamily, SubtitleFontFamily.Value);

            if (SubtitleDelayInSeconds.HasValue)
                patch.Replace(p => p.SubtitleDelayInSeconds, SubtitleDelayInSeconds.Value);

            if (LoadFirstSubtitleFoundAutomatically.HasValue)
                patch.Replace(p => p.LoadFirstSubtitleFoundAutomatically, LoadFirstSubtitleFoundAutomatically.Value);

            return patch;
        }
    }
}
