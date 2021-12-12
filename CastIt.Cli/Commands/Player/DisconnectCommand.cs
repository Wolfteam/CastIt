using CastIt.Cli.Interfaces.Api;
using CastIt.Shared.Server;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "disconnect", Description = "Disconnects from the current connected device and stops the web server", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class DisconnectCommand : BaseCommand
    {
        [Option(CommandOptionType.NoValue, Description = "Indicates if the server should be killed, defaults to false", LongName = "kill-server")]
        public bool KillServer { get; set; }

        public DisconnectCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            AppConsole.WriteLine("Disconnecting from the current device and killing the web server...");
            if (WebServerUtils.IsServerAlive())
            {
                var response = await CastItApi.Disconnect();
                CheckServerResponse(response);
            }

            if (KillServer)
            {
                AppConsole.WriteLine("Killing server process...");
                WebServerUtils.KillServerProcess();
            }

            AppConsole.WriteLine("Disconnection completed");
            return SuccessCode;
        }
    }
}
