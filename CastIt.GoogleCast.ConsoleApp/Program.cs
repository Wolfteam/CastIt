using CastIt.GoogleCast;
using CastIt.GoogleCast.Models.Media;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestPlayer().GetAwaiter().GetResult();
        }

        private static async Task TestPlayer()
        {
            var devices = await Player.GetDevicesAsync();
            if (devices.Count == 0)
            {
                Console.WriteLine("No devices were found");
            }
            var device = devices.First();
            var player = new Player(device, logMsgs: false);
            player.Disconnected += (e, sender) =>
            {
                Console.WriteLine("DISCONNECTED");
            };
            player.EndReached += (sender, e) =>
            {
                Console.WriteLine($"END REACHED");
            };

            player.Init();
            await player.ConnectAsync();
            await player.LoadAsync(new MediaInformation
            {
                ContentId = @"http://192.168.1.101:9696/videos?seconds=0&file=F:\Videos\Da Capo Opening 1.mp4",
                Duration = 90
            });


            Console.WriteLine("Tap to pause");
            Console.ReadKey();
            await player.PauseAsync();


            Console.WriteLine("Tap to play");
            Console.ReadKey();
            await player.PlayAsync();


            Console.WriteLine("Type any key to disconnect");
            Console.ReadKey();
            await player.DisconnectAsync();
        }
    }
}
