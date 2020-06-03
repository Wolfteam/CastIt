using CastIt.Common.Utils;
using CastIt.Interfaces;
using EmbedIO;
using EmbedIO.Actions;
using MvvmCross.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CastIt.Server
{
    public static class AppWebServer
    {
        private const string VideosPath = "/videos";
        private const string ImagesPath = "/images";
        private const string SubTitlesPath = "/subtitles";

        public const string SecondsQueryParameter = "seconds";
        public const string FileQueryParameter = "file";
        public const string VideoStreamIndexParameter = "videoStream";
        public const string AudioStreamIndexParameter = "audioStream";
        //public const string SubTitleStreamIndexParameter = "subtitleStream";

        public static IReadOnlyList<string> AllowedQueryParameters => new List<string>
        {
            SecondsQueryParameter,
            FileQueryParameter,
            VideoStreamIndexParameter,
            AudioStreamIndexParameter
        };

        public static WebServer CreateWebServer(
            IMvxLogProvider logger,
            IFFMpegService ffmpegService)
        {
            var url = GetIpAddress();
            string previewPath = FileUtils.GetPreviewsPath();
            string subtitlesPath = FileUtils.GetSubTitleFolder();

            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithCors()
                .WithStaticFolder(ImagesPath, previewPath, false)
                .WithStaticFolder(SubTitlesPath, subtitlesPath, false)
                .WithModule(new VideoModule(logger.GetLogFor<VideoModule>(), ffmpegService, VideosPath))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Server initialized" })));
            //if a clients is disconected this throws an exception
            server.Listener.IgnoreWriteExceptions = false;
            return server;
        }

        public static string GetMediaUrl(WebServer webServer, string filePath, int videoStreamIndex, int audioStreamIndex, double seconds)
        {
            var baseUrl = GetBaseUrl(webServer);
            return $"{baseUrl}{VideosPath}?" +
                $"{VideoStreamIndexParameter}={videoStreamIndex}" +
                $"&{AudioStreamIndexParameter}={audioStreamIndex}" +
                $"&{SecondsQueryParameter}={seconds}" +
                $"&{FileQueryParameter}={Uri.EscapeDataString(filePath)}";
        }

        public static string GetPreviewPath(WebServer webServer, string filepath)
        {
            var baseUrl = GetBaseUrl(webServer);
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public static string GetSubTitlePath(WebServer webServer, string filepath)
        {
            var baseUrl = GetBaseUrl(webServer);
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{SubTitlesPath}/{Uri.EscapeDataString(filename)}";
        }

        private static string GetBaseUrl(WebServer webServer)
            => webServer.Options.UrlPrefixes.First();

        private static string GetIpAddress()
        {
            string localIP = null;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }

                var port = GetOpenPort();

                return $"http://{localIP}:{port}";
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static int GetOpenPort(int startPort = 9696)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

            return Enumerable.Range(startPort, 99).FirstOrDefault(port => !usedPorts.Contains(port));
        }
    }
}
