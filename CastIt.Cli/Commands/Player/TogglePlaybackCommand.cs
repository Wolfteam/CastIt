using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "toggle-playback", Description = "Toggles the playback of the current played file", OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    public class TogglePlaybackCommand : BaseCommand
    {
        public TogglePlaybackCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            AppConsole.WriteLine("Toggling playback of current played file...");
            var response = await CastItApi.TogglePlayback();
            CheckServerResponse(response);

            AppConsole.WriteLine("Playback was successfully toggled");
            return SuccessCode;
        }
    }
}
