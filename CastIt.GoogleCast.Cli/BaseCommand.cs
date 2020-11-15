using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli
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
