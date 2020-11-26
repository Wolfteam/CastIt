using CastIt.Application.Server;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "connect", Description = "Connects to a particular device")]
    public class ConnectCommand : BaseCommand
    {
        private readonly IConsole _console;

        [Argument(0, Description = "The device´s ip address", ShowInHelpText = true)]
        public string IpAddress { get; set; }

        public ConnectCommand(IConsole console)
        {
            _console = console;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            _console.WriteLine($"Connecting to {IpAddress}...");
            try
            {
                if (!WebServerUtils.IsServerAlive())
                {
                    _console.WriteLine("Server is not running");
                    return -1;
                }

                if (string.IsNullOrWhiteSpace(IpAddress) || !IpAddress.Contains(":"))
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }

                var splitted = IpAddress.Split(':');
                bool hostIsValid = IPAddress.TryParse(splitted[0], out var deviceHost);
                bool portWasParsed = int.TryParse(splitted[1], out int devicePort);
                if (!portWasParsed || !hostIsValid)
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }

                var url = ServerUtils.StartServerIfNotStarted(_console);
                if (string.IsNullOrWhiteSpace(url))
                    return -1;

                _console.WriteLine($"Trying to call the connect api of the web server = {url}");
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.Connect(deviceHost.ToString(), devicePort);
                _console.WriteLine(response.Succeed
                    ? "Connection completed"
                    : $"Connection to web server failed. Error = {response.Message}");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }

            return await base.OnExecute(app);
        }
    }
}
