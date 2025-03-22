using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "getall", Description = "Retrieves a list with all the playlists", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class GetAllCommand : BaseCommand
    {
        public GetAllCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            AppConsole.WriteLine("Retrieving play lists...");

            var response = await CastItApi.GetAllPlayLists();
            CheckServerResponse(response);

            var playLists = response.Result;

            AppConsole.WriteLine("The following play lists were found:");
            var table = new ConsoleTable("Id", "Name", "Files", "Loop", "Shuffle", "Position", "Played Time");

            foreach (var playList in playLists)
            {
                table.AddRow(playList.Id, playList.Name, playList.NumberOfFiles, playList.Loop, playList.Shuffle, playList.Position, playList.TotalDuration);
            }

            AppConsole.WriteLine(table.ToString());
            return SuccessCode;
        }
    }
}
