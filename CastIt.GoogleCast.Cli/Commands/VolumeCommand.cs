using CastIt.Application.Server;
using CastIt.GoogleCast.Cli.Common.Utils;
using CastIt.GoogleCast.Cli.Interfaces.Api;
using McMaster.Extensions.CommandLineUtils;
using Refit;
using System;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli.Commands
{
    [Command(Name = "volume", Description = "Sets the volume of the device")]
    public class VolumeCommand : BaseCommand
    {
        private readonly IConsole _console;

        [Option(CommandOptionType.NoValue, Description = "Pass this option to mute the device.", LongName = "muted")]
        public bool IsMuted { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, Description = "The volume level of the device, it has to be a value between 0 - 100. Defaults to 100", LongName = "level")]
        public float Volume { get; set; } = 100;

        public VolumeCommand(IConsole console)
        {
            _console = console;
        }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                if (!WebServerUtils.IsServerAlive())
                {
                    _console.WriteLine("Server is not running");
                    return -1;
                }

                _console.WriteLine($"Setting volume to = {Volume} and muted = {IsMuted}...");
                var url = ServerUtils.StartServerIfNotStarted(_console);
                var castItApi = RestService.For<ICastItApi>(url);
                var response = await castItApi.SetVolume(Volume, IsMuted);
                if (!response.Succeed)
                {
                    _console.WriteLine(response.Message);
                    return -1;
                }

                _console.WriteLine("Volume was successfully updated");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }
            return await base.OnExecute(app);
        }
    }
}
