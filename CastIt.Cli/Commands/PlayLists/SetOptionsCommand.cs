using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.PlayLists
{
    [Command(Name = "set-options", Description = "Sets playlist options", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class SetOptionsCommand : BaseCommand
    {
        [Argument(0, Description = "The playlist id", ShowInHelpText = true)]
        public long PlayListId { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Sets if the playlist should loop", LongName = "loop", ShortName = "loop")]
        public bool Loop { get; set; }

        [Option(CommandOptionType.NoValue, Description = "Sets if the playlist should shuffle", LongName = "shuffle", ShortName = "shuffle")]
        public bool Shuffle { get; set; }

        public SetOptionsCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            //CheckIfWebServerIsRunning();
            var response = await CastItApi.SetOptions(PlayListId, Loop, Shuffle);
            CheckServerResponse(response);

            AppConsole.WriteLine("Playlist was successfully updated");
            return SuccessCode;
        }
    }
}
