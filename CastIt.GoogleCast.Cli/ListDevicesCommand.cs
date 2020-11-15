using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli
{
    [Command(Name = "devices", Description = "List the available devices in the network")]
    public class ListDevicesCommand : BaseCommand
    {
        private readonly IConsole _console;

        [Option(CommandOptionType.SingleOrNoValue, Description = "The device's scan timeout in seconds", LongName = "timeout")]
        public int Timeout { get; set; } = 10;

        public ListDevicesCommand(IConsole console)
        {
            _console = console;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            _console.WriteLine($"Getting devices. The timeout is = {Timeout} second(s)....");
            var devices = await Player.GetDevicesAsync(TimeSpan.FromSeconds(Timeout));
            if (devices.Count == 0)
            {
                _console.WriteLine("No devices were found");
                return await base.OnExecute(app);
            }

            _console.WriteLine("The following devices were found:");
            var table = new ConsoleTable("Id", "FriendlyName", "Type", "IpAddress");

            foreach (var device in devices)
            {
                table.AddRow(device.Id, device.FriendlyName, device.Type, $"{device.Host}:{device.Port}");
            }

            _console.WriteLine(table.ToString());
            return await base.OnExecute(app);
        }
    }
}
