using McMaster.Extensions.CommandLineUtils;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Cli
{
    [Command(Name = "connect", Description = "Connects to a particular device or to the first one if no value is provided")]
    public class ConnectCommand : BaseCommand
    {
        private readonly IConsole _console;
        private readonly Player _player;
        public ConnectCommand(IConsole console, Player player)
        {
            _console = console;
            _player = player;
        }

        [Argument(0, Description = "The device´s ip address", ShowInHelpText = true)]
        public string IpAddress { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            _console.WriteLine($"Connecting to {IpAddress}...");
            try
            {
                if (!IpAddress.Contains(":"))
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }

                var splitted = IpAddress.Split(':');
                bool hostIsValid = IPAddress.TryParse(splitted[0], out var address);
                bool portWasParsed = int.TryParse(splitted[1], out int port);
                if (!portWasParsed || !hostIsValid)
                {
                    _console.WriteLine($"The provided ip address = {IpAddress} is not valid");
                    return await base.OnExecute(app);
                }
                //TODO: THE CONNECT METHOD TRIES TO START THE RECEIVER LOOPS
                await _player.ConnectAsync(address.ToString(), port);

                _console.WriteLine("Connection completed");
            }
            catch (Exception e)
            {
                _console.WriteLine(e.ToString());
            }

            return await base.OnExecute(app);
        }
    }
}
