using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "stop", Description = "Stops the playback of the current played file", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class StopCommand : BaseCommand
    {
        public StopCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            AppConsole.WriteLine("Stopping playback of current played file...");
            var response = await CastItApi.Stop();
            CheckServerResponse(response);

            AppConsole.WriteLine("Playback was successfully stopped");

            return SuccessCode;
        }
    }
}
