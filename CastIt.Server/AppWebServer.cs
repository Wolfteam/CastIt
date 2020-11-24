using CastIt.Application.Interfaces;
using CastIt.Application.Server;
using CastIt.GoogleCast.Interfaces;
using CastIt.Infrastructure.Interfaces;
using CastIt.Server.Common;
using CastIt.Server.Controllers;
using CastIt.Server.Interfaces;
using CastIt.Server.Modules;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace CastIt.Server
{
    public class AppWebServer : BaseWebServer, IAppWebServer
    {
        #region Members
        //public const string SubTitleStreamIndexParameter = "subtitleStream";
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<AppWebServer> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly IFFmpegService _ffmpegService;
        private readonly IFileService _fileService;
        private readonly IPlayer _player;
        private readonly MediaModule _mediaModule;

        private CastItController _castItController;
        private WebServer _webServer;
        private bool _disposed;
        #endregion

        public AppWebServer(
            ILoggerFactory loggerFactory,
            ITelemetryService telemetryService,
            IFFmpegService ffmpegService,
            IFileService fileService,
            IPlayer player)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<AppWebServer>();
            _telemetryService = telemetryService;
            _ffmpegService = ffmpegService;
            _fileService = fileService;
            _player = player;
            _mediaModule = new MediaModule(_logger, ffmpegService, telemetryService, fileService, AppWebServerConstants.MediaPath);
        }

        #region Methods
        public void Init(
            string previewPath,
            string subtitlesPath,
            IViewForMediaWebSocket view,
            ICastService castService,
            CancellationToken cancellationToken,
            int? port = null)
        {
            try
            {
                if (port.HasValue)
                {
                    _logger.LogInformation($"{nameof(Init)}: Port = {port} was provided, it will be used to start this web server...");
                }
                port ??= WebServerUtils.GetOpenPort();
                _logger.LogInformation($"{nameof(Init)}: Starting web server on url = {WebServerUtils.GetWebServerIpAddress(port.Value)}...");

                _castItController = new CastItController(_loggerFactory.CreateLogger<CastItController>(), _fileService, castService, _ffmpegService, _player);
                _webServer = BuildServer(previewPath, subtitlesPath, view, port.Value);
                _webServer.Start(cancellationToken);

                _logger.LogInformation($"{nameof(Init)}: Server was successfully started");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(Init)}: Unknown error");
                _telemetryService.TrackError(e);
                throw;
            }
        }

        public void Init(string previewPath, string subtitlesPath, ICastService castService, CancellationToken cancellationToken, int? port = null)
            => Init(previewPath, subtitlesPath, null, castService, cancellationToken, port);

        public override void Dispose()
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

        protected override string GetBaseUrl()
        {
            if (_webServer != null)
                return _webServer.Options.UrlPrefixes.First();
            _logger.LogError($"{nameof(GetBaseUrl)}: Web server is null!");
            throw new NullReferenceException("Web server is null");
        }

        private WebServer BuildServer(string previewPath, string subtitlesPath, IViewForMediaWebSocket view, int port)
        {
            var url = WebServerUtils.GetWebServerIpAddress(port);
            _logger.LogInformation(
                $"{nameof(BuildServer)}: Building server on url = {url}, " +
                $"using path for previews = {previewPath} and for subs = {subtitlesPath}");
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithCors()
                .WithStaticFolder(AppWebServerConstants.ImagesPath, previewPath, false)
                .WithStaticFolder(AppWebServerConstants.SubTitlesPath, subtitlesPath, false)
                .WithWebApi("/api", m => m.WithController(() => _castItController))
                .WithModule(_mediaModule);

            if (view != null)
            {
                _logger.LogInformation($"{nameof(BuildServer)}: View for socket was provided, adding the media ws");
                server.WithModule(new MediaWebSocketModule(_loggerFactory.CreateLogger<MediaWebSocketModule>(), this, view, "/socket"));
            }

            server.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Server initialized" })));

            //if a clients is disconected this throws an exception
            server.Listener.IgnoreWriteExceptions = false;
            return server;
        }
        #endregion
    }
}
