using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "delete", Description = "Deletes the playlist", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class DeleteCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        public DeleteCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
            var response = await CastItApi.DeletePlayList(PlayListId);
            CheckServerResponse(response);

            AppConsole.WriteLine($"PlayListId = {PlayListId} was successfully deleted");
            return SuccessCode;
        }
    }
}
