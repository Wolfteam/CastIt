using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Enums;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "sort-files", Description = "Sorts the play list files", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class SortFilesCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The sort mode", LongName = "mode", ShortName = "mode")]
        public SortModeType SortMode { get; set; }

        public SortFilesCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            if (PlayListId <= 0)
            {
                AppConsole.WriteLine("Invalid playListId");
                return ErrorCode;
            }

            var response = await CastItApi.SortFiles(PlayListId, SortMode);
            CheckServerResponse(response);
            return SuccessCode;
        }
    }
}
