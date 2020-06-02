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
            //await player.LoadAsync(new MediaInformation
            //{
            //    ContentId = @"http://192.168.1.101:9696/videos?seconds=0&file=F:\Videos\Da Capo Opening 1.mp4",
            //    Duration = 90
            //});

            await player.LoadAsync(new MediaInformation
            {
                ContentId = "https://www.youtube.com/watch?v=O2mEccTaxiw",
                ContentType = "video/webm"
            });

            Console.WriteLine("Tap to pause");
            Console.ReadKey();
            await player.PauseAsync();

            Console.WriteLine("Tap to play");
            Console.ReadKey();
            await player.PlayAsync();

            Console.WriteLine("Tap to seek 30 seconds");
            Console.ReadKey();
            await player.SeekAsync(30);

            Console.WriteLine("Type any key to disconnect");
            Console.ReadKey();
            player.Dispose();

            Console.WriteLine("Type any key to close this app");
            Console.ReadKey();
        }
    }
}
