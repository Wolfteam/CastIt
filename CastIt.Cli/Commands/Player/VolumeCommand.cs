using CastIt.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace CastIt.Cli.Commands.Player
{
    [Command(Name = "volume", Description = "Sets the volume of the device")]
    public class VolumeCommand : BaseCommand
    {
        [Option(CommandOptionType.NoValue, Description = "Pass this option to mute the device.", LongName = "muted")]
        public bool IsMuted { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The volume level of the device, it has to be a value between 0 - 100. Defaults to 100", LongName = "level")]
        public float Volume { get; set; } = 100;

        public VolumeCommand(IConsole appConsole, ICastItApiService castItApi)
            : base(appConsole, castItApi)
        {
        }

        protected override async Task<int> Execute(CommandLineApplication app)
        {
            CheckIfWebServerIsRunning();

            AppConsole.WriteLine($"Setting volume to = {Volume} and muted = {IsMuted}...");
            var response = await CastItApi.SetVolume(Volume, IsMuted);
            CheckServerResponse(response);

            AppConsole.WriteLine("Volume was successfully updated");
            return SuccessCode;
        }
    }
}
