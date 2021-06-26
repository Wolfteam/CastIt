﻿using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Files
{
    [Command(Name = "play", Description = "Plays a particular file to the connected device", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class PlayCommand : BaseCommand
    {
        public PlayCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        [Argument(0, Description = "The play list id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Argument(1, Description = "The file id", ShowInHelpText = true)]
        public long FileId { get; set; }

        //[Argument(0, Description = "The file's full path or url", ShowInHelpText = true)]
        //public string Mrl { get; set; }

        //[Option(CommandOptionType.SingleOrNoValue, Description = "The audio stream index, defaults to 0", LongName = "audio_index", ShortName = "ai")]
        //public int AudioStreamIndex { get; set; }

        //[Option(CommandOptionType.SingleOrNoValue, Description = "The video stream index, defaults to 0", LongName = "video_index", ShortName = "vi")]
        //public int VideoStreamIndex { get; set; }

        //[Option(CommandOptionType.SingleOrNoValue, Description = "The subtitles stream index, defaults to -1", LongName = "subs_index", ShortName = "si")]
        //public int SubsStreamIndex { get; set; } = -1;

        //[Option(CommandOptionType.SingleOrNoValue, Description = "The video quality, only required for youtube videos", LongName = "quality")]
        //public int Quality { get; set; } = 360;

        //[Option(CommandOptionType.SingleOrNoValue, Description = "The seconds where the file will be started", LongName = "seconds", ShortName = "sec")]
        //public int Seconds { get; set; }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
            if (PlayListId <= 0 || FileId <= 0)
            {
                AppConsole.WriteLine($"PlaylistId = {PlayListId} or FileId = {FileId} are not valid");
                return ErrorCode;
            }

            var response = await CastItApi.Play(PlayListId, FileId);
            CheckServerResponse(response);
            AppConsole.WriteLine($"FileId = {FileId} was successfully loaded");
            return SuccessCode;
        }
    }
}