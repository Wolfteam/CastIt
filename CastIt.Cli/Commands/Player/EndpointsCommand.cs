using CastIt.Cli.Interfaces.Api;
using ConsoleTables;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "endpoints", Description = "Manage custom endpoint devices", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class EndpointsCommand : BaseCommand
    {
        [Option(CommandOptionType.NoValue, Description = "Adds a new custom endpoint", LongName = "add", ShortName = "add")]
        public bool Add { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Deletes the custom endpoint with the provided id", LongName = "delete", ShortName = "delete")]
        public long? Delete { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Updates the custom endpoint with the provided id", LongName = "update", ShortName = "update")]
        public long? Update { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The custom endpoint's name", LongName = "name", ShortName = "name")]
        public string Name { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The custom endpoint's host", LongName = "host", ShortName = "host")]
        public string Host { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "The custom endpoint's port", LongName = "port", ShortName = "port")]
        public int Port { get; set; } = 8009;

        public EndpointsCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            if (Add)
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Host))
                {
                    AppConsole.WriteLine("The --name and --host options are required to add a custom endpoint");
                    return ErrorCode;
                }

                AppConsole.WriteLine($"Adding custom endpoint = {Name} ({Host}:{Port})...");
                var addResponse = await CastItApi.AddCustomEndpoint(Name, Host, Port);
                CheckServerResponse(addResponse);
                AppConsole.WriteLine("Custom endpoint was successfully added");
                return SuccessCode;
            }

            if (Update.HasValue)
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Host))
                {
                    AppConsole.WriteLine("The --name and --host options are required to update a custom endpoint");
                    return ErrorCode;
                }

                AppConsole.WriteLine($"Updating custom endpoint id = {Update.Value}...");
                var updateResponse = await CastItApi.UpdateCustomEndpoint(Update.Value, Name, Host, Port);
                CheckServerResponse(updateResponse);
                AppConsole.WriteLine("Custom endpoint was successfully updated");
                return SuccessCode;
            }

            if (Delete.HasValue)
            {
                AppConsole.WriteLine($"Deleting custom endpoint id = {Delete.Value}...");
                var deleteResponse = await CastItApi.DeleteCustomEndpoint(Delete.Value);
                CheckServerResponse(deleteResponse);
                AppConsole.WriteLine("Custom endpoint was successfully deleted");
                return SuccessCode;
            }

            AppConsole.WriteLine("Trying to retrieve the custom endpoints from server...");
            var response = await CastItApi.GetCustomEndpoints();
            CheckServerResponse(response);

            var endpoints = response.Result;
            if (endpoints.Count == 0)
            {
                AppConsole.WriteLine("No custom endpoints were found");
                return SuccessCode;
            }

            AppConsole.WriteLine("The following custom endpoints were found:");
            var table = new ConsoleTable("Id", "Name", "IpAddress");

            foreach (var endpoint in endpoints)
            {
                var id = endpoint.Id?.Replace("endpoint:", string.Empty);
                table.AddRow(id, endpoint.FriendlyName, $"{endpoint.Host}:{endpoint.Port}");
            }

            AppConsole.WriteLine(table.ToString());

            AppConsole.WriteLine("Search completed");
            return SuccessCode;
        }
    }
}
