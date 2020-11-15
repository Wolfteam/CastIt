using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using CastIt.Server.Common;
using CastIt.Server.Controllers;
using CastIt.Server.Interfaces;
using CastIt.Server.Modules;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
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
        #region Members
        //public const string SubTitleStreamIndexParameter = "subtitleStream";
        private readonly IMvxLog _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly IFFmpegService _ffmpegService;
        private readonly IFileService _fileService;

        private WebServer _webServer;
        private bool _disposed;
        #endregion

        #region Events
        public OnFileLoadingHandler OnFileLoading { get; set; }
        public OnFileLoadedWsHandler OnFileLoaded { get; set; }
        public OnFileLoadingErrorHandler OnFileLoadingError { get; set; }
        public OnPositionChangedHandler OnPositionChanged { get; set; }
        public OnTimeChangedHandler OnTimeChanged { get; set; }
        public OnEndReachedHandler OnEndReached { get; set; }
        public OnPausedHandler OnPaused { get; set; }
        public OnDisconnectedHandler OnDisconnected { get; set; }
        public OnVolumeChangedHandler OnVolumeChanged { get; set; }
        public OnAppClosingHandler OnAppClosing { get; set; }
        public OnAppSettingsChangedHandler OnAppSettingsChanged { get; set; }

        public OnPlayListAddedHandler OnPlayListAdded { get; set; }
        public OnPlayListChangedHandler OnPlayListChanged { get; set; }
        public OnPlayListDeletedHandler OnPlayListDeleted { get; set; }

        public OnFileAddedHandler OnFileAdded { get; set; }
        public OnFileChangedHandler OnFileChanged { get; set; }
        public OnFileDeletedHandler OnFileDeleted { get; set; }

        public OnServerMsgHandler OnServerMsg { get; set; }
        #endregion

        #region Properties
        public static IReadOnlyList<string> AllowedQueryParameters => AppWebServerConstants.AllowedQueryParameters;

        public string BaseUrl
            => GetBaseUrl();
        #endregion

        public AppWebServer(
            IMvxLogProvider logger,
            ITelemetryService telemetryService,
            IFFmpegService ffmpegService,
            IFileService fileService)
        {
            _logger = logger.GetLogFor<AppWebServer>();
            _telemetryService = telemetryService;
            _ffmpegService = ffmpegService;
            _fileService = fileService;
        }

        #region Methods
        public void Init(string previewPath, string subtitlesPath, IViewForMediaWebSocket view, CancellationToken cancellationToken)
        {
            try
            {
                _webServer = BuildServer(previewPath, subtitlesPath, view);
                _webServer.Start(cancellationToken);

                _logger.Info($"{nameof(Init)}: Server was started");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(Init)}: Unknown error");
                _telemetryService.TrackError(e);
            }
        }

        public void Init(string previewPath, string subtitlesPath, CancellationToken cancellationToken)
            => Init(previewPath, subtitlesPath, null, cancellationToken);

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

        public string GetMediaUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            string videoWidthAndHeight = null)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}{AppWebServerConstants.MediaPath}?" +
                $"{AppWebServerConstants.VideoStreamIndexParameter}={videoStreamIndex}" +
                $"&{AppWebServerConstants.AudioStreamIndexParameter}={audioStreamIndex}" +
                $"&{AppWebServerConstants.SecondsQueryParameter}={seconds}" +
                $"&{AppWebServerConstants.FileQueryParameter}={Uri.EscapeDataString(filePath)}" +
                $"&{AppWebServerConstants.VideoNeedsTranscode}={videoNeedsTranscode}" +
                $"&{AppWebServerConstants.AudioNeedsTranscode}={audioNeedsTranscode}" +
                $"&{AppWebServerConstants.HwAccelTypeToUse}={hwAccelToUse}" +
                $"&{AppWebServerConstants.VideoWidthAndHeight}={videoWidthAndHeight}";
        }

        public string GetPreviewPath(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.ImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public string GetSubTitlePath(string filepath)
        {
            var baseUrl = GetBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}{AppWebServerConstants.SubTitlesPath}/{Uri.EscapeDataString(filename)}";
        }

        private string GetBaseUrl()
        {
            if (_webServer != null)
                return _webServer.Options.UrlPrefixes.First();
            _logger.Error($"{nameof(GetBaseUrl)}: Web server is null!");
            throw new NullReferenceException("Web server is null");
        }

        private string GetIpAddress()
        {
            string localIp = null;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
            }

            var port = GetOpenPort();

            return $"http://{localIp}:{port}";
        }

        private int GetOpenPort(int startPort = AppWebServerConstants.DefaultPort)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEndPoints = properties.GetActiveTcpListeners();
            var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

            return Enumerable.Range(startPort, 99).FirstOrDefault(port => !usedPorts.Contains(port));
        }

        private WebServer BuildServer(string previewPath, string subtitlesPath, IViewForMediaWebSocket view)
        {
            var url = GetIpAddress();
            _logger.Info($"{nameof(BuildServer)}: Building server on url = {url}");
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithCors()
                .WithStaticFolder(AppWebServerConstants.ImagesPath, previewPath, false)
                .WithStaticFolder(AppWebServerConstants.SubTitlesPath, subtitlesPath, false)
                .WithWebApi("/api", m => m.WithController(() => new CastItController(_fileService)))
                .WithModule(new MediaModule(_logger, _ffmpegService, _telemetryService, _fileService, AppWebServerConstants.MediaPath))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Server initialized" })));

            if (view != null)
            {
                _logger.Info($"{nameof(BuildServer)}: View for socket was provided, adding the media ws");
                server.WithModule(new MediaWebSocketModule(_logger, this, view, "/socket"));
            }
            //if a clients is disconected this throws an exception
            server.Listener.IgnoreWriteExceptions = false;
            return server;
        }
        #endregion
    }
}
