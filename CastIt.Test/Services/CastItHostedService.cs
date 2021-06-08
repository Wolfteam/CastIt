using System.IO;
using System.Linq;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Interfaces;
using CastIt.Test.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using CastIt.Application.Interfaces;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace CastIt.Test.Services
{
    public class CastItHostedService : BackgroundService
    {
        private readonly ILogger<CastItHostedService> _logger;
        private readonly IServerCastService _castService;
        private readonly IHubContext<CastItHub, ICastItHub> _castItHub;
        private readonly IAppSettingsService _appSettings;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly IServer _server;

        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();

        public CastItHostedService(
            ILogger<CastItHostedService> logger,
            IServerCastService castService,
            IHubContext<CastItHub, ICastItHub> castItHub,
            IAppSettingsService appSettings,
            IFileWatcherService fileWatcherService,
            IServer server)
        {
            _logger = logger;
            _castService = castService;
            _castItHub = castItHub;
            _appSettings = appSettings;
            _fileWatcherService = fileWatcherService;
            _server = server;

            _castService.OnCastableDeviceAdded = OnDeviceDiscovered;
            _castService.OnFileLoaded = OnFileLoaded;
            _castService.OnServerMessage = OnServerMessage;
            _castService.OnPositionChanged = OnPositionChanged;
            _castService.OnEndReached = OnEndReached;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var serverAddressesFeature = _server.Features.Get<IServerAddressesFeature>();
            _logger.LogInformation("Initializing on the following ip: " + string.Join(", ", serverAddressesFeature.Addresses));

            _logger.LogInformation("Initializing app settings...");
            _appSettings.Init();

            _logger.LogInformation("Initialization completed");
            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service is going down!");
            _setDurationTokenSource.Cancel();
            _castItHub.Clients.All.ShutDown();
            _castService.CleanThemAll();
            //TODO: USE EVENTS AND REMOVE THEM HERE
            return base.StopAsync(cancellationToken);
        }

        private void InitializeOrUpdateFileWatcher(bool update)
        {
            _logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Getting directories to watch...");
            var dirs = _castService.PlayLists.SelectMany(pl => pl.Files)
                .Where(f => f.IsLocalFile)
                .Select(f => Path.GetDirectoryName(f.Path))
                .Distinct()
                .ToList();

            _logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Got = {dirs.Count} directories...");
            if (!update)
            {
                _logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Starting to watch for {dirs.Count} directories...");
                _fileWatcherService.StartListening(dirs);
                _fileWatcherService.OnFileCreated = OnFwCreated;
                _fileWatcherService.OnFileChanged = OnFwChanged;
                _fileWatcherService.OnFileDeleted = OnFwDeleted;
                _fileWatcherService.OnFileRenamed = OnFwRenamed;
            }
            else
            {
                _logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Updating watched directories...");
                _fileWatcherService.UpdateWatchers(dirs);
            }
        }

        #region FW handlers
        private Task OnFwCreated(string path, bool isAFolder)
        {
            return OnFwChanged(path, isAFolder);
        }

        private async Task OnFwChanged(string path, bool isAFolder)
        {
            var files = _castService.PlayLists
                .SelectMany(f => f.Files)
                .Where(f => isAFolder ? f.Path.StartsWith(path) : f.Path == path)
                .ToList();
            foreach (var file in files)
            {
                var playlist = _castService.PlayLists.Find(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;

                //await playlist.SetFileInfo(file.Id, _setDurationTokenSource.Token);
                //_appWebServer?.OnFileChanged(playlist.Id);
            }
        }

        private Task OnFwDeleted(string path, bool isAFolder)
        {
            return OnFwChanged(path, isAFolder);
        }

        private async Task OnFwRenamed(string oldPath, string newPath, bool isAFolder)
        {
            var files = _castService.PlayLists
                .SelectMany(f => f.Files)
                .Where(f => isAFolder ? f.Path.StartsWith(oldPath) : f.Path == oldPath)
                .ToList();
            foreach (var file in files)
            {
                var playlist = _castService.PlayLists.FirstOrDefault(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;

                //if (isAFolder)
                //{
                //    //Here I'm not sure how to retain the order
                //    await playlist.RemoveFilesThatStartsWith(oldPath);
                //    await playlist.OnFolderAddedCommand.ExecuteAsync(new[] { newPath });
                //}
                //else
                //{
                //    await playlist.OnFilesAddedCommand.ExecuteAsync(new[] { newPath });
                //    playlist.ExchangeLastFilePosition(file.Id);
                //    await playlist.RemoveFile(file.Id);
                //}
                //_appWebServer?.OnPlayListChanged(playlist.Id);
            }
        }
        #endregion

        private void OnDeviceDiscovered(string id, string name, string type, string host, int port)
        {
            _logger.LogInformation($"Found deviceId = {id}");
        }

        private async void OnServerMessage(AppMessageType type)
        {
            await _castItHub.Clients.All.SendMsg(type);
        }

        private async void OnPositionChanged(double newPosition)
        {
            await _castItHub.Clients.All.PositionChanged(newPosition);
        }

        private async void OnFileLoaded(string title, string path, double duration, double volume, bool muted)
        {
            _logger.LogInformation($"File Loaded = {title}");
        }

        private async void OnEndReached()
        {
            await _castItHub.Clients.All.EndReached();
        }
    }
}
