using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace CastIt.GoogleCast.Cli.Commands
{
    [HelpOption("--help", ShortName = "h")]
    public abstract class BaseCommand
    {
        protected virtual Task<int> OnExecute(CommandLineApplication app)
        {
            return Task.FromResult(0);
        }
    }
}
