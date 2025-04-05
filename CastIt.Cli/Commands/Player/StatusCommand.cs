using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "status", Description = "Retrieves info about the player status, played file, etc", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class StatusCommand : BaseCommand
    {
        public StatusCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            var response = await CastItApi.GetStatus();
            CheckServerResponse(response);
            //just to avoid printing a lot of stuff...
            response.Result.ThumbnailRanges.Clear();
            PrettyPrintAsJson(response.Result);

            return SuccessCode;
        }
    }
}
