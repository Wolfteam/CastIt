using CastIt.Application.Server;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.ServiceProcess;
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
            if (!WebServerUtils.IsElevated())
            {
                AppConsole.WriteLine("You need to run this command as an administrator");
                return ErrorCode;
            }

            if (Restart)
            {
                RestartServer();
            }
            else if (Stop)
            {
                StopServer();
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

            StartOrStopService(true, false);
            return WebServerUtils.IsServerAlive();
        }

        private void StopServer()
        {
            AppConsole.WriteLine("Stopping server...");

            if (WebServerUtils.IsServerAlive())
            {
                StartOrStopService(false, true);
                AppConsole.WriteLine("Server was successfully stopped");
            }
            else
            {
                AppConsole.WriteLine("Server cannot be stopped since it is not running");
            }
        }

        private void RestartServer()
        {
            AppConsole.WriteLine("Restarting server...");
            StartOrStopService(true, true);
            if (WebServerUtils.IsServerAlive())
            {
                AppConsole.WriteLine("Server was successfully restarted");
            }
        }

        public void StartOrStopService(bool start, bool stop)
        {
            if (!OperatingSystem.IsWindows())
            {
                AppConsole.WriteLine("Operation is not supported");
                return;
            }

            var serviceController = Array.Find(ServiceController.GetServices(), s => s.ServiceName == WebServerUtils.ServerProcessName);
            if (serviceController == null)
            {
                AppConsole.WriteLine("The server service is not installed");
                return;
            }

            if (stop)
            {
                serviceController.Stop();
            }

            if (start)
            {
                serviceController.Start();
            }
        }
    }
}
