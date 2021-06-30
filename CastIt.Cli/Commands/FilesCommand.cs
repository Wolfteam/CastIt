using CastIt.Cli.Commands.Files;
using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace CastIt.Cli.Commands
{
    [Command(Name = "files", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [Subcommand(
        typeof(UpdateCommand)
    )]
    public class FilesCommand : BaseCommand
    {
        public FilesCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }
    }
}
