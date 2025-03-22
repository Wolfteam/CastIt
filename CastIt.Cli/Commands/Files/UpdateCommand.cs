using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Files
{
    [Command(Name = "update", Description = "Updates a file", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class UpdateCommand : BaseCommand
    {
        [Argument(0, Description = "The play list id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Argument(1, Description = "The file id", ShowInHelpText = true)]
        public long FileId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Set this one to loop the file", LongName = "loop", ShortName = "loop")]
        public bool? Loop { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The file position", LongName = "position", ShortName = "position")]
        public int? Position { get; set; }

        public UpdateCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            if (Loop.HasValue)
            {
                AppConsole.WriteLine($"Updating file to loop = {Loop}");
                var response = await CastItApi.LoopFile(PlayListId, FileId, Loop.Value);
                CheckServerResponse(response);
            }

            if (Position >= 0)
            {
                AppConsole.WriteLine($"Updating file position to = {Position}");
                var response = await CastItApi.UpdateFilePosition(PlayListId, FileId, Position.Value);
                CheckServerResponse(response);
            }

            AppConsole.WriteLine("File was successfully updated");
            return SuccessCode;
        }
    }
}
