using CastIt.Cli.Commands.Files;
using CastIt.Cli.Commands.Player;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands
{
    [Command(Name = "player", Description = "Allows you to interact with the player", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(
        typeof(ConnectCommand),
        typeof(DisconnectCommand),
        typeof(DevicesCommand),
        typeof(PlayCommand),
        typeof(TogglePlaybackCommand),
        typeof(StopCommand),
        typeof(VolumeCommand),
        typeof(GoToCommand),
        typeof(SettingsCommand),
        typeof(StatusCommand),
        typeof(SetCurrentPlayedFileOptionsCommand)
    )]
    public class PlayerCommands : BaseCommand
    {
        public PlayerCommands(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return base.OnExecute(app);
        }
    }
}
