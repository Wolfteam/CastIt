using CastIt.Application.Server;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [Command(Name = "server", Description = "Starts, Stops or Restarts the server", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class ServerCommands : BaseCommand
    {
        [Option(CommandOptionType.NoValue, Description = "Pass this one to start the server if it hasn't been started already", LongName = "start", ShortName = "start")]
        public bool Start { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The port were the server will run on. It only applies to the start / restart command", LongName = "port", ShortName = "port")]
        public int? Port { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Pass this one to restart the server", LongName = "restart", ShortName = "restart")]
        public bool Restart { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Pass this one to stop the server", LongName = "stop", ShortName = "stop")]
        public bool Stop { get; set; }

        public ServerCommands(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            if (Restart)
            {
                await RestartServer();
            }
            else if (Stop)
            {
                await StopServer();
            }
            else if (Start)
            {
                StartServer();
            }

            return await base.Execute(app);
        }

        private bool StartServer()
        {
            if (WebServerUtils.IsServerAlive())
            {
                AppConsole.WriteLine("Server is already started");
                return true;
            }

            var port = Port ?? WebServerUtils.GetOpenPort();
            var args = $"{AppWebServerConstants.PortArgument} {port}";
            bool started = WebServerUtils.StartServer(args, WebServerUtils.GetServerPhysicalPath());
            if (!started)
            {
                AppConsole.WriteLine($"Server could not be started on port = {port}");
            }

            return started;
        }

        private async Task StopServer()
        {
            AppConsole.WriteLine("Stopping server...");

            if (WebServerUtils.IsServerAlive())
            {
                var response = await CastItApi.StopServer();
                CheckServerResponse(response);
                AppConsole.WriteLine("Server was successfully stopped");
            }
            else
            {
                AppConsole.WriteLine("Server cannot be stopped since it is not running");
            }
        }

        private async Task RestartServer()
        {
            AppConsole.WriteLine("Restarting server...");
            await StopServer();
            bool started = StartServer();

            if (started)
            {
                AppConsole.WriteLine("Server was successfully restarted");
            }
        }
    }
}
