using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "status", Description = "Retrieves info about the player status, played file, etc")]
    public class StatusCommand : BaseCommand
    {
        public StatusCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            var response = await CastItApi.GetStatus();
            CheckServerResponse(response);
            PrettyPrintAsJson(response.Result);

            return SuccessCode;
        }
    }
}
