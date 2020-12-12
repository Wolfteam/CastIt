using CastIt.Application.Server;
using CastIt.Cli.Common.Utils;
using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
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

            try
            {
                if (!WebServerUtils.IsServerAlive())
                {
                    _console.WriteLine("Server is not running");
                    return -1;
                }

                var url = ServerUtils.StartServerIfNotStarted(_console);
                if (string.IsNullOrWhiteSpace(url))
                    return -1;

                _console.WriteLine($"Trying to retrieve the available devices from api using server url = {url}");
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.GetAllDevices(Timeout);
                if (!response.Succeed)
                {
                    _console.WriteLine($"Error = {response.Message}");
                    return -1;
                }

                var devices = response.Result;
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

                _console.WriteLine("Search completed");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.Message);
            }

            return await base.OnExecute(app);
        }
    }
}
