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
        [Option(CommandOptionType.SingleValue, Description = "The play list id", LongName = "playlist-id", ShortName = "playlist-id")]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The playlist name", LongName = "name", ShortName = "name")]
        public string Name { get; set; }

        public GetCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            if (PlayListId <= 0 && string.IsNullOrWhiteSpace(Name))
            {
                AppConsole.WriteLine($"Either PlayListId = {PlayListId} or the Name = {Name} is not valid");
                return ErrorCode;
            }

            var response = await CastItApi.GetPlayList(PlayListId, Name);
            CheckServerResponse(response);

            var table = new ConsoleTable("Id", "Name", "Information", "Position", "Playing", "Played Time");
            foreach (var file in response.Result.Files)
            {
                table.AddRow(file.Id, file.Filename, file.SubTitle, file.Position, file.IsBeingPlayed ? "Yes" : "No", file.TotalDuration);
            }
            AppConsole.WriteLine(table.ToString());
            return SuccessCode;
        }
    }
}
