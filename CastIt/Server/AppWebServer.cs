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

        public const string SecondsQueryParameter = "seconds";
        public const string FileQueryParameter = "file";

        public static IReadOnlyList<string> AllowedQueryParameters => new List<string>
        {
            SecondsQueryParameter,
            FileQueryParameter
        };

        public static WebServer CreateWebServer(
            IMvxLogProvider logger,
            IFFMpegService ffmpegService)
        {
            var url = GetIpAddress();
            string previewPath = FileUtils.GetPreviewsPath();

            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithStaticFolder(ImagesPath, previewPath, false)
                .WithModule(new VideoModule(logger.GetLogFor<VideoModule>(), ffmpegService, VideosPath))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx =>
                {
                    return ctx.SendDataAsync(new { Message = "Server initialized" });
                }));
            //if a clients is disconected this throws an exception
            server.Listener.IgnoreWriteExceptions = false;
            return server;
        }

        public static string GetMediaUrl(WebServer webServer, string filePath, double seconds)
        {
            var baseUrl = webServer.Options.UrlPrefixes.First();

            return $"{baseUrl}{VideosPath}?{SecondsQueryParameter}={seconds}&{FileQueryParameter}={Uri.EscapeDataString(filePath)}";
        }

        public static string GetPreviewPath(WebServer webServer, string filepath)
        {
            var baseUrl = webServer.Options.UrlPrefixes.First();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

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
