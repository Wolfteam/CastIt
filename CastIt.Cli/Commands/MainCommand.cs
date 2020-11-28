using McMaster.Extensions.CommandLineUtils;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [Command(Name = "castit", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(StartServerCommand),
        typeof(ConnectCommand),
        typeof(DisconnectCommand),
        typeof(ListDevicesCommand),
        typeof(PlayCommand),
        typeof(TogglePlaybackCommand),
        typeof(StopCommand),
        typeof(VolumeCommand),
        typeof(SettingsCommand)
        )]
    public class MainCommand : BaseCommand
    {
        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return base.OnExecute(app);
        }

        private static string GetVersion()
            => typeof(MainCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }
}
