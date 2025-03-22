using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "new", Description = "Creates a new playlist", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class NewCommand : BaseCommand
    {
        public NewCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            var response = await CastItApi.AddNewPlayList();
            CheckServerResponse(response);

            AppConsole.WriteLine($"PlayListId = {response.Result.Id} was successfully created");
            return SuccessCode;
        }
    }
}
