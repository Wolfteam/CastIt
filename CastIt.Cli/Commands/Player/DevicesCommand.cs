using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "devices", Description = "List the available devices in the network", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class DevicesCommand : BaseCommand
    {
        [Option(
            CommandOptionType.NoValue,
            Description = "Refreshes the devices in the server. The refresh will last for the specified seconds in the Timeout property",
            LongName = "refresh",
            ShortName = "refresh")]
        public bool Refresh { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The device's scan timeout in seconds", LongName = "refresh-timeout", ShortName = "refresh-timeout")]
        public int RefreshTimeout { get; set; } = 10;

        public DevicesCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            if (Refresh)
            {
                AppConsole.WriteLine($"Refreshing devices with a refreshTimeout = {RefreshTimeout} ...");
                var refreshResponse = await CastItApi.RefreshDevices(RefreshTimeout);
                CheckServerResponse(refreshResponse);
            }

            AppConsole.WriteLine("Trying to retrieve the available devices from server...");
            var response = await CastItApi.GetAllDevices();
            CheckServerResponse(response);

            var devices = response.Result;
            if (devices.Count == 0)
            {
                AppConsole.WriteLine("No devices were found");
                return SuccessCode;
            }

            AppConsole.WriteLine("The following devices were found:");
            var table = new ConsoleTable("Id", "FriendlyName", "Type", "IpAddress");

            foreach (var device in devices)
            {
                table.AddRow(device.Id, device.FriendlyName, device.Type, $"{device.Host}:{device.Port}");
            }

            AppConsole.WriteLine(table.ToString());

            AppConsole.WriteLine("Search completed");
            return SuccessCode;
        }
    }
}
