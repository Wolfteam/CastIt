using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "devices", Description = "List the available devices in the network")]
    public class ListDevicesCommand : BaseCommand
    {
        //TODO: BRING BACK THE TIMEOUT THING
        //[Option(CommandOptionType.SingleOrNoValue, Description = "The device's scan timeout in seconds", LongName = "timeout")]
        //public int Timeout { get; set; } = 10;

        public ListDevicesCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();
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
