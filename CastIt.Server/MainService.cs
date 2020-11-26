using CastIt.Application.Interfaces;
using CastIt.Infrastructure.Interfaces;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server
{
    internal class MainService : BackgroundService
    {
        private readonly ILogger<MainService> _logger;
        private readonly IAppWebServer _webServer;
        private readonly ICastService _castService;
        private readonly IAppSettingsService _appSettings;
        private readonly IFileService _fileService;
        private readonly int _startingPort;

        public MainService(
            ILogger<MainService> logger,
            IAppWebServer webServer,
            ICastService castService,
            IAppSettingsService appSettings,
            IFileService fileService,
            int startingPort)
        {
            _logger = logger;
            _webServer = webServer;
            _startingPort = startingPort;
            _fileService = fileService;
            _appSettings = appSettings;
            _castService = castService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Service started at {DateTimeOffset.Now}");
            _appSettings.Init();
            _fileService.DeleteServerLogsAndPreviews();
            _webServer.Init(_fileService.GetPreviewsPath(), _fileService.GetSubTitleFolder(), _castService, stoppingToken, _startingPort);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Service stopped at: {DateTimeOffset.Now}");
            _webServer.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}
