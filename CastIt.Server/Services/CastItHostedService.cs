using CastIt.Application.Interfaces;
using CastIt.Domain.Enums;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public class CastItHostedService : BackgroundService
    {
        private readonly ILogger<CastItHostedService> _logger;
        private readonly IServerCastService _castService;
        private readonly IHubContext<CastItHub, ICastItHub> _castItHub;
        private readonly IServerAppSettingsService _appSettings;
        private readonly IFileWatcherService _fileWatcherService;
        private readonly IBaseWebServer _baseWebServer;
        private readonly IFileService _fileService;

        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();

        public CastItHostedService(
            ILogger<CastItHostedService> logger,
            IServerCastService castService,
            IHubContext<CastItHub, ICastItHub> castItHub,
            IServerAppSettingsService appSettings,
            IFileWatcherService fileWatcherService,
            IFileService fileService,
            IBaseWebServer baseWebServer)
        {
            _logger = logger;
            _castService = castService;
            _castItHub = castItHub;
            _appSettings = appSettings;
            _fileWatcherService = fileWatcherService;
            _fileService = fileService;
            _baseWebServer = baseWebServer;

            _castService.OnCastableDeviceAdded = OnDeviceDiscovered;
            _castService.OnFileLoaded = OnFileLoaded;
            _castService.OnServerMessage = OnServerMessage;
            _castService.OnPositionChanged = OnPositionChanged;
            _castService.OnEndReached = OnEndReached;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _fileService.DeleteServerLogsAndPreviews();
            _baseWebServer.Init();

            _logger.LogInformation("Initializing app settings...");
            await _appSettings.Init();

            _logger.LogInformation("Initialization completed");
            await base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hosted service is going down!");
            _setDurationTokenSource.Cancel();
            await _castService.CleanThemAll();
            await _castItHub.Clients.All.ShutDown();
            //TODO: USE EVENTS AND REMOVE THEM HERE
            await base.StopAsync(cancellationToken);
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
