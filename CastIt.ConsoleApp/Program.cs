using CastIt.GoogleCast;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Models.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ConsoleApp
{
    class Program
    {
        private static Player _player;
        static void Main(string[] args)
        {
            TestPlayer().GetAwaiter().GetResult();
        }

        private static async Task TestPlayer()
        {
            var devices = await Player.GetDevicesAsync(TimeSpan.FromSeconds(3));
            if (devices.Count == 0)
            {
                Console.WriteLine("No devices were found");
                Console.ReadLine();
                return;
            }
            var device = devices.First();
            _player = new Player(device, logMsgs: false);
            _player.Disconnected += (e, sender) =>
            {
                Console.WriteLine("DISCONNECTED");
            };
            _player.EndReached += (sender, e) =>
            {
                Console.WriteLine($"END REACHED");
            };

            _player.Init();
            await _player.ConnectAsync();
            bool canSeek = false;

            await PlayFromLocal();

            //await PlayFromLocalWithSubs();

            Console.WriteLine("Tap to pause");
            Console.ReadKey();
            await _player.PauseAsync();

            Console.WriteLine("Tap to play");
            Console.ReadKey();
            await _player.PlayAsync();

            if (canSeek)
            {
                Console.WriteLine("Tap to seek 30 seconds");
                Console.ReadKey();
                await _player.SeekAsync(30);
            }

            Console.WriteLine("Type any key to disconnect");
            Console.ReadKey();
            await _player.StopPlaybackAsync();
            _player.Dispose();

            Console.WriteLine("Type any key to close this app");
            Console.ReadKey();
        }

        private static  Task PlayFromLocal()
        {
            return _player.LoadAsync(new MediaInformation
            {
                ContentId = @"http://192.168.1.101:9696/videos?seconds=400&file=F:\Anime\Asobi%20Asobase\Asobi%20Asobase%201.mp4",
                StreamType = StreamType.Live,
                Duration = 1200
            });
        }

        private static Task PlayFromLocalWithSubs()
        {
            var mediaInfo = new MediaInformation
            {
                ContentId = @"http://192.168.1.101:9696/videos?seconds=600&file=F:\Movies\John%20Wick\John%20Wick%203%20Parabellum.mkv",
                ContentType = "video/mp4",
                StreamType = StreamType.Live,
                Metadata = new MovieMetadata
                {
                    Title = "John wick",
                },
                Tracks = new List<Track>
                {
                    new Track
                    {
                        TrackId = 1,
                        SubType = TextTrackType.Subtitles,
                        Type = TrackType.Text,
                        Name = "English",
                        TrackContentId = "http://192.168.1.101:9696/subtitles/subs.vtt",
                        Language = "en-US"
                    }
                },
                TextTrackStyle = new TextTrackStyle
                {
                    BackgroundColor = Color.Transparent,
                    EdgeColor = Color.Black,
                    FontScale = 1.2F,
                    WindowType = TextTrackWindowType.Normal,
                    EdgeType = TextTrackEdgeType.None,
                    FontStyle = TextTrackFontStyleType.Normal,
                    FontGenericFamily = TextTrackFontGenericFamilyType.Casual,
                }
            };

            return _player.LoadAsync(mediaInfo, true, 0, 360, 1);
        }
    }
}
