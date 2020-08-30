using CastIt.GoogleCast;
using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Models;
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
            Console.WriteLine("Getting devices....");
            var devices = await Player.GetDevicesAsync(TimeSpan.FromSeconds(10));
            if (devices.Count == 0)
            {
                Console.WriteLine("No devices were found");
                Console.ReadLine();
                return;
            }
            var device = devices.First();
            Console.WriteLine($"Device to use will be = {device.FriendlyName}");

            _player = new Player(device, logMsgs: true);
            _player.Disconnected += (e, sender) =>
            {
                Console.WriteLine("DISCONNECTED");
            };
            _player.EndReached += (sender, e) =>
            {
                Console.WriteLine($"END REACHED");
            };
            _player.LoadFailed += (sender, e) =>
            {
                Console.WriteLine($"LOAD FAILED");
            };

            Console.WriteLine($"Connecting to device = {device.FriendlyName}....");
            _player.Init();
            await _player.ConnectAsync();
            bool canSeek = false;

            //await PlayFromLocal();

            //await PlayFromLocalWithSubs();

            await PlayFromHls();

            Console.WriteLine("File loaded, tap to pause");
            Console.ReadKey();
            await _player.PauseAsync();

            Console.WriteLine("File paused, tap to play");
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
            await _player.DisconnectAsync();
            _player.Dispose();
            Console.WriteLine("Type any key to close this app");
            Console.ReadKey();
        }

        private static Task PlayFromLocal()
        {
            Console.WriteLine($"Playing from local file...");
            return _player.LoadAsync(new MediaInformation
            {
                ContentId = @"http://192.168.1.101:9696/videos?seconds=400&file=F:\Anime\Asobi%20Asobase\Asobi%20Asobase%201.mp4&videoNeedsTranscode=False&audioNeedsTranscode=False&hwAccelTypeToUse=Nvidia&videoWidthAndHeight=1280x720",
                StreamType = StreamType.Live,
                Duration = 1200
            });
        }

        private static Task PlayFromLocalWithSubs()
        {
            Console.WriteLine($"Playing from local file with subs...");
            var mediaInfo = new MediaInformation
            {
                ContentId = @"http://192.168.1.101:9696/videos?seconds=40&file=F:\Movies\John%20Wick\John%20Wick%203%20Parabellum.mkv&videoNeedsTranscode=False&audioNeedsTranscode=False&hwAccelTypeToUse=Nvidia&videoWidthAndHeight=1280x720",
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

        private static Task PlayFromHls()
        {
            Console.WriteLine($"Playing from hls");
            var mediaInfo = new MediaInformation
            {
                //ContentId = "https://edge.flowplayer.org/bauhaus.m3u8",
                //Duration = -1,
                //ContentType = "application/vnd.apple.mpegurl",
                //ContentType = "application/x-mpegurl",
                //ContentType = "application/dash+xml",
                ContentId = "http://192.168.1.101:9696/videos?seconds=40&file=https://commondatastorage.googleapis.com/gtv-videos-bucket/CastVideos/hls/DesigningForGoogleCast.m3u8&videoNeedsTranscode=False&audioNeedsTranscode=False&hwAccelTypeToUse=Nvidia&videoWidthAndHeight=1280x720",
                StreamType = StreamType.Live,
                ContentType = "video/mp4",
                Metadata = new GenericMediaMetadata
                {
                    Title = "This is the title",
                    Subtitle = "This is the subtitle",
                    Images = new List<Image>
                    {
                        new Image
                        {
                            Url = "https://i.ytimg.com/vi/wHn1_QVoXGM/maxresdefault_live.jpg"
                        }
                    }
                }
            };
            return _player.LoadAsync(mediaInfo, true, 0, new List<int>().ToArray());
        }
    }
}
