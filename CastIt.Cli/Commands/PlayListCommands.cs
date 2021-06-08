using CastIt.Cli.Commands.PlayLists;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace CastIt.Cli.Commands
{
    [Command(Name = "playlists", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(
        typeof(NewCommand),
        typeof(AddCommand),
        typeof(DeleteAllExceptCommand),
        typeof(DeleteCommand),
        typeof(GetAllCommand),
        typeof(GetCommand),
        typeof(RemoveFilesCommand),
        typeof(SetOptionsCommand),
        typeof(UpdateCommand)
    )]
    public class PlayListCommands : BaseCommand
    {
        public PlayListCommands(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }
    }
}
