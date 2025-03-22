using CastIt.Cli.Interfaces.Api;
using CastIt.Shared.Server;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading.Tasks;
using CastIt.Cli.Models;
using CastIt.Domain.Utils;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CastIt.Cli.Commands;

[Command(
    Name = "configure",
    Description = "Configures the connection to the server",
    OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
public class ConfigureCommand : BaseCommand
{
    private readonly AppSettings _appSettings;

    [Option(CommandOptionType.SingleOrNoValue,
        Description = "The url where the server is running",
        LongName = "url",
        ShortName = "url")]
    public string Url { get; set; }

    [Option(CommandOptionType.SingleOrNoValue,
        Description = "Shows the current configuration",
        LongName = "show",
        ShortName = "show")]
    public bool Show { get; set; }

    public ConfigureCommand(IConsole appConsole, ICastItApiService castItApi, AppSettings appSettings)
        : base(appConsole, castItApi)
    {
        _appSettings = appSettings;
    }

    protected override Task<int> OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
        return base.OnExecute(app);
    }

    protected override async Task<int> Execute(CommandLineApplication app)
    {
        if (Show)
        {
            PrettyPrintAsJson(_appSettings);
            return SuccessCode;
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out _))
        {
            AppConsole.WriteLine("The provided url is not valid");
            return ErrorCode;
        }

        if (_appSettings.ServerUrl != Url)
        {
            _appSettings.ServerUrl = Url;
            await _appSettings.Save();
        }

        return SuccessCode;
    }
}