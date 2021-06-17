using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "get", Description = "Retrieves the playlist + it's files", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class GetCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        public GetCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            if (PlayListId <= 0)
            {
                AppConsole.WriteLine($"PlayListId = {PlayListId} is not valid");
                return ErrorCode;
            }

            var response = await CastItApi.GetPlayList(PlayListId);
            CheckServerResponse(response);

            var table = new ConsoleTable("Id", "Name", "Information", "Playing", "Played Time");
            foreach (var file in response.Result.Files)
            {
                table.AddRow(file.Id, file.Filename, file.SubTitle, file.IsBeingPlayed ? "Yes" : "No", file.TotalDuration);
            }
            AppConsole.WriteLine(table.ToString());
            return SuccessCode;
        }
    }
}
