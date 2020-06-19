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
using System.Threading;

namespace CastIt.Server
{
    public class AppWebServer : IAppWebServer
    {
        private const string MediaPath = "/media";
        private const string ImagesPath = "/images";
        private const string SubTitlesPath = "/subtitles";
        private const int DefaultPort = 9696;

        public const string SecondsQueryParameter = "seconds";
        public const string FileQueryParameter = "file";
        public const string VideoStreamIndexParameter = "videoStream";
        public const string AudioStreamIndexParameter = "audioStream";
        //public const string SubTitleStreamIndexParameter = "subtitleStream";

        private readonly IMvxLog _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly IFFMpegService _ffmpegService;

        private WebServer _webServer;
        private bool _disposed;

        public static IReadOnlyList<string> AllowedQueryParameters => new List<string>
        {
            SecondsQueryParameter,
            FileQueryParameter,
            VideoStreamIndexParameter,
            AudioStreamIndexParameter
        };

        public AppWebServer(
            IMvxLogProvider logger,
            ITelemetryService telemetryService,
            IFFMpegService ffmpegService)
        {
            _logger = logger.GetLogFor<AppWebServer>();
            _telemetryService = telemetryService;
            _ffmpegService = ffmpegService;
        }

        public void Init(CancellationToken cancellationToken)
        {
            try
            {
                var url = GetIpAddress();
                string previewPath = FileUtils.GetPreviewsPath();
                string subtitlesPath = FileUtils.GetSubTitleFolder();

                _logger.Info($"{nameof(Init)}: Starting server on url = {url}");
                var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                    .WithLocalSessionManager()
                    .WithCors()
                    .WithStaticFolder(ImagesPath, previewPath, false)
                    .WithStaticFolder(SubTitlesPath, subtitlesPath, false)
                    .WithModule(new VideoModule(_logger, _ffmpegService, _telemetryService, MediaPath))
                    .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Server initialized" })));
                //if a clients is disconected this throws an exception
                server.Listener.IgnoreWriteExceptions = false;
                _webServer = server;
                _webServer.Start(cancellationToken);

                _logger.Info($"{nameof(Init)}: Server was started");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(Init)}: Unknown error");
                _telemetryService.TrackError(e);
            }
        }

        public void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed state (managed objects).
                _webServer?.Dispose();
            }

            _disposed = true;
        }

        public string GetMediaUrl(string filePath, int videoStreamIndex, int audioStreamIndex, double seconds)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}{MediaPath}?" +
                $"{VideoStreamIndexParameter}={videoStreamIndex}" +
                $"&{AudioStreamIndexParameter}={audioStreamIndex}" +
                $"&{SecondsQueryParameter}={seconds}" +
                $"&{FileQueryParameter}={Uri.EscapeDataString(filePath)}";
        }

        public string GetPreviewPath(string filepath)
        {
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public string GetSubTitlePath(string filepath)
        {
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{SubTitlesPath}/{Uri.EscapeDataString(filename)}";
        }

        private string GetBaseUrl()
        {
            if (_webServer is null)
            {
                _logger.Error($"{nameof(GetBaseUrl)}: Web server is null!");
                throw new NullReferenceException("Web server is null");
            }
            return _webServer.Options.UrlPrefixes.First();
        }

        private string GetIpAddress()
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

        private int GetOpenPort(int startPort = DefaultPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

            return Enumerable.Range(startPort, 99).FirstOrDefault(port => !usedPorts.Contains(port));
        }
    }
}
