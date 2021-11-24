using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "set-file-options", Description = "Sets the file options of the current played file", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class SetCurrentPlayedFileOptionsCommand : BaseCommand
    {
        [Option(CommandOptionType.SingleOrNoValue, Description = "The audio stream index, defaults to 0", LongName = "audio-index", ShortName = "audio-index")]
        public int AudioStreamIndex { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The subtitles stream index, defaults to -1", LongName = "subs-index", ShortName = "subs-index")]
        public int SubsStreamIndex { get; set; } = -1;

        [Option(CommandOptionType.SingleOrNoValue, Description = "The video quality, only required for youtube videos", LongName = "quality", ShortName = "quality")]
        public int Quality { get; set; }

        public SetCurrentPlayedFileOptionsCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            var response = await CastItApi.SetCurrentPlayedFileOptions(AudioStreamIndex, SubsStreamIndex, Quality);
            CheckServerResponse(response);
            return SuccessCode;
        }
    }
}
