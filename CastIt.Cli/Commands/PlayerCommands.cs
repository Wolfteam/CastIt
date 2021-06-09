using CastIt.Cli.Commands.Player;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace CastIt.Cli.Commands
{
    [Command(Name = "player", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(
        typeof(StartServerCommand),
        typeof(ConnectCommand),
        typeof(DisconnectCommand),
        typeof(ListDevicesCommand),
        typeof(PlayCommand),
        typeof(TogglePlaybackCommand),
        typeof(StopCommand),
        typeof(VolumeCommand),
        typeof(GoToCommand),
        typeof(SettingsCommand)
    )]
    public class PlayerCommands : BaseCommand
    {
        public PlayerCommands(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }
    }
}
