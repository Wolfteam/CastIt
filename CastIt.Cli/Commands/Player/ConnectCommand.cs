using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "connect", Description = "Connects to a particular device", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class ConnectCommand : BaseCommand
    {
        [Argument(0, Description = "The device´s ip address", ShowInHelpText = true)]
        public string IpAddress { get; set; }

        public ConnectCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            AppConsole.WriteLine($"Connecting to {IpAddress}...");
            CheckIfWebServerIsRunning();

            if (string.IsNullOrWhiteSpace(IpAddress) || !IpAddress.Contains(":"))
            {
                AppConsole.WriteLine($"The provided ip address = {IpAddress} is not valid");
                return ErrorCode;
            }

            var splitted = IpAddress.Split(':');
            bool hostIsValid = IPAddress.TryParse(splitted[0], out var deviceHost);
            bool portWasParsed = int.TryParse(splitted[1], out int devicePort);
            if (!portWasParsed || !hostIsValid)
            {
                AppConsole.WriteLine($"The provided ip address = {IpAddress} is not valid");
                return ErrorCode;
            }

            AppConsole.WriteLine("Trying to call the connect api of the web server...");
            var response = await CastItApi.Connect(deviceHost.ToString(), devicePort);

            CheckServerResponse(response);
            AppConsole.WriteLine("Connection completed");
            return SuccessCode;
        }
    }
}
