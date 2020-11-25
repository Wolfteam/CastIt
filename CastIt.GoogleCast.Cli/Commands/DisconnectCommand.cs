using CastIt.Application.Server;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "disconnect", Description = "Disconnects from the current connected device and stops the web server")]
    public class DisconnectCommand : BaseCommand
    {
        private readonly IConsole _console;

        public DisconnectCommand(IConsole console)
        {
            _console = console;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                _console.WriteLine("Disconnecting from the current device and killing the web server...");
                if (WebServerUtils.IsServerAlive())
                {
                    var url = ServerUtils.StartServerIfNotStarted(_console);
                    var castItApi = RestService.For<ICastItApi>(url);
                    var response = await castItApi.Disconnect();
                    if (!response.Succeed)
                    {
                        _console.WriteLine(response.Message);
                        return -1;
                    }
                }
                _console.WriteLine("Killing server process...");
                WebServerUtils.KillServerProcess();

                _console.WriteLine("Disconnection completed");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }
            return await base.OnExecute(app);
        }
    }
}
