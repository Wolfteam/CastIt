﻿using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Server.Hubs;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly ITelemetryService _telemetryService;

        private readonly CancellationTokenSource _setDurationTokenSource = new CancellationTokenSource();

        public CastItHostedService(
            ILogger<CastItHostedService> logger,
            IServerCastService castService,
            IHubContext<CastItHub, ICastItHub> castItHub,
            IServerAppSettingsService appSettings,
            IFileWatcherService fileWatcherService,
            IFileService fileService,
            IBaseWebServer baseWebServer,
            ITelemetryService telemetryService)
        {
            _logger = logger;
            _castService = castService;
            _castItHub = castItHub;
            _appSettings = appSettings;
            _fileWatcherService = fileWatcherService;
            _fileService = fileService;
            _baseWebServer = baseWebServer;
            _telemetryService = telemetryService;

            SetCastItEventHandlers();
        }

        private void SetCastItEventHandlers()
        {
            _castService.OnCastRendererSet = OnCastDeviceSet;
            _castService.OnCastableDeviceAdded = OnCastDeviceDiscovered;
            _castService.OnCastableDeviceDeleted = OnCastableDeviceDeleted;
            _castService.OnCastDevicesChanged = OnCastDevicesChanged;
            _castService.OnFileLoading = OnFileLoading;
            _castService.OnFileLoaded = OnFileLoaded;
            _castService.OnPositionChanged = OnPositionChanged;
            _castService.OnTimeChanged = OnTimeChanged;
            _castService.OnEndReached = OnEndReached;
            _castService.QualitiesChanged = QualitiesChanged;
            _castService.OnPaused = OnPaused;
            _castService.OnDisconnected = OnCastDeviceDisconnected;
            _castService.OnVolumeChanged = OnVolumeChanged;
            _castService.OnServerMessage = OnServerMessage;
            _castService.OnAppClosing = OnAppClosing;
            _castService.OnStoppedPlayback = OnStoppedPlayback;

            _castService.OnPlayListAdded = OnPlayListAdded;
            _castService.OnPlayListChanged = OnPlayListChanged;
            _castService.OnPlayListDeleted = OnPlayListDeleted;
            _castService.OnPlayListBusy = OnPlayListBusy;
            _castService.OnFileAdded = OnFileAdded;
            _castService.OnFileChanged = OnFileChanged;
            _castService.OnFileDeleted = OnFileDeleted;
            _castService.OnFilesAdded = OnFilesAdded;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(StartAsync)}: Initializing castit service...");
            _fileService.DeleteServerLogsAndPreviews();
            _baseWebServer.Init();

            _logger.LogInformation($"{nameof(StartAsync)}: Initializing castit service...");
            await _castService.Init();

            _logger.LogInformation($"{nameof(StartAsync)}: Initializing app settings...");
            await _appSettings.Init();

            _logger.LogInformation($"{nameof(StartAsync)}: Initializing file watchers...");
            InitializeOrUpdateFileWatcher(false);

            _logger.LogInformation($"{nameof(StartAsync)}: Initialization completed");
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(ExecuteAsync)}: Refreshing play lists images...");
            _castService.RefreshPlayListImages();

            _logger.LogInformation($"{nameof(ExecuteAsync)}: Setting missing file info for pending files...");
            await _castService.SetFileInfoForPendingFiles();

            UpdateFileWatchers();
            _logger.LogInformation($"{nameof(ExecuteAsync)}: Process completed");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            //TODO: THIS MSG IS NOT GETTING DELIVERED
            _logger.LogInformation($"{nameof(StopAsync)}: Hosted service is going down!");
            await _castItHub.Clients.All.ServerMessage(AppMessageType.ServerIsClosing);

            _logger.LogInformation($"{nameof(StopAsync)}: Cancelling any pending duration job...");
            _setDurationTokenSource.Cancel();

            _logger.LogInformation($"{nameof(StopAsync)}: Stop listening to folders...");
            _fileWatcherService.StopListening();

            _logger.LogInformation($"{nameof(StopAsync)}: Cleaning the cast service...");
            await _castService.CleanThemAll();

            _logger.LogInformation($"{nameof(StopAsync)}: Saving current settings...");
            await _appSettings.SaveCurrentSettings();

            _logger.LogInformation($"{nameof(StopAsync)}: Stop completed");
            await base.StopAsync(cancellationToken);
        }

        private void InitializeOrUpdateFileWatcher(bool update)
        {
            _logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Getting directories to watch...");
            var dirs = GetFoldersToWatch();

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

        private List<string> GetFoldersToWatch()
        {
            var dirs = _castService.PlayLists.SelectMany(pl => pl.Files)
                .Where(f => f.IsLocalFile)
                .Select(f => Path.GetDirectoryName(f.Path))
                .Distinct()
                .ToList();
            return dirs;
        }

        //AKA UpdatePlayedTime
        private void UpdateFileWatchers()
        {
            var dirs = GetFoldersToWatch();
            _fileWatcherService.UpdateWatchers(dirs, false);
            //TODO: PLAYLISTS CHANGED
        }

        #region CastIt Handlers
        private async void OnFileDeleted(long playListId, long id)
        {
            await _castItHub.Clients.All.FileDeleted(playListId, id);
        }

        private async void OnFileChanged(FileItemResponseDto file)
        {
            await _castItHub.Clients.All.FileChanged(file);
        }

        private async void OnFileAdded(FileItemResponseDto file)
        {
            await _castItHub.Clients.All.FileAdded(file);
        }

        private async void OnPlayListDeleted(long id)
        {
            await _castItHub.Clients.All.PlayListDeleted(id);
        }

        private async void OnPlayListAdded(GetAllPlayListResponseDto playList)
        {
            await _castItHub.Clients.All.PlayListAdded(playList);
        }

        private async void OnPlayListChanged(GetAllPlayListResponseDto playList)
        {
            await _castItHub.Clients.All.PlayListChanged(playList);
        }

        private async void OnPlayListBusy(long id, bool busy)
        {
            await PlayListBusy(id, busy);
        }

        private void OnAppClosing()
        {
        }

        private async void OnFileLoading(FileItemResponseDto file)
        {
            _logger.LogInformation($"{nameof(OnFileLoading)}: File = {file.Name} is being loaded...");
            await _castItHub.Clients.All.FileLoading(file);
        }

        private async void OnVolumeChanged(double newLevel, bool isMuted)
        {
            await PlayerStatusChanged();
        }

        private async void OnCastDeviceDisconnected()
        {
            _logger.LogInformation($"{nameof(OnCastDeviceDisconnected)}: A disconnect took place");
            await _castItHub.Clients.All.CastDeviceDisconnected();
        }

        private async void OnPaused()
        {
            await PlayerStatusChanged();
        }

        private void QualitiesChanged(int selectedQuality, List<int> qualities)
        {
        }

        private void OnTimeChanged(double seconds)
        {
            _castService.CurrentPlayedFile?.UpdatePlayedSeconds(seconds);
        }

        private void OnCastableDeviceDeleted(string id)
        {
        }

        private async void OnCastDeviceSet(IReceiver device)
        {
            await _castItHub.Clients.All.CastDeviceSet(device);
        }

        private async void OnCastDeviceDiscovered(IReceiver device)
        {
            _logger.LogInformation($"Found deviceId = {device.Id}");
            await _castItHub.Clients.All.CastDevicesChanged(_castService.AvailableDevices);
        }

        private async void OnServerMessage(AppMessageType type)
        {
            _logger.LogInformation($"{nameof(OnServerMessage)}: Sending server msg = {type} to all clients");
            await _castItHub.Clients.All.ServerMessage(type);
        }

        private async void OnPositionChanged(double newPosition)
        {
            _castService.CurrentPlayedFile?.UpdatePlayedPercentage(newPosition);
            await PlayerStatusChanged();
        }

        private async void OnFileLoaded(FileItemResponseDto file)
        {
            _logger.LogInformation($"{nameof(OnFileLoaded)}: File {file.Filename} was loaded");
            await _castItHub.Clients.All.FileLoaded(file);
            await PlayerStatusChanged();
        }

        private async void OnEndReached()
        {
            _logger.LogInformation($"{nameof(OnEndReached)}: End reached for = {_castService.CurrentPlayedFile?.Filename}");
            _castService.CurrentPlayedFile?.EndReached();
            await _castItHub.Clients.All.FileEndReached(_castService.GetCurrentPlayedFile());
            _castService.CleanPlayedFile(false);
            await PlayerStatusChanged();
        }

        private async void OnFilesAdded(long playListId, FileItem[] files)
        {
            _logger.LogInformation($"{nameof(OnFilesAdded)}: {files.Length} file(s) were added for playListId = {playListId}... Adding them...");
            try
            {
                await PlayListBusy(playListId, true);
                foreach (var file in files)
                {
                    if (_setDurationTokenSource.IsCancellationRequested)
                        break;
                    await _castService.AddFile(playListId, file);
                }

                _logger.LogInformation($"{nameof(OnFilesAdded)}: Successfully added all the files to playListId = {playListId}");
            }
            catch (Exception e)
            {
                await _castItHub.Clients.All.ServerMessage(AppMessageType.UnknownErrorOccurred);
                _telemetryService.TrackError(e);
                _logger.LogError(e, $"{nameof(OnFilesAdded)}: Unknown error");
            }
            finally
            {
                await PlayListBusy(playListId, false);
            }
        }

        private async void OnStoppedPlayback()
        {
            await _castItHub.Clients.All.StoppedPlayBack();
            await PlayerStatusChanged();
        }

        private async void OnCastDevicesChanged(List<IReceiver> devices)
        {
            await _castItHub.Clients.All.CastDevicesChanged(devices);
        }

        private Task PlayerStatusChanged()
        {
            return _castItHub.Clients.All.PlayerStatusChanged(_castService.GetPlayerStatus());
        }

        private async Task PlayListBusy(long id, bool isBusy)
        {
            await _castItHub.Clients.All.PlayListIsBusy(id, isBusy);
        }

        #endregion

        #region FileWatcher Handlers
        private Task OnFwCreated(string path, bool isAFolder)
        {
            return OnFwChanged(path, isAFolder);
        }

        private async Task OnFwChanged(string path, bool isAFolder)
        {
            foreach (var playList in _castService.PlayLists)
            {
                var files = playList.Files
                    .Where(f => isAFolder ? f.Path.StartsWith(path) : f.Path == path)
                    .ToList();

                if (files.Any())
                {
                    continue;
                }

                await _castItHub.Clients.All.PlayListIsBusy(playList.Id, true);

                foreach (var file in files)
                {
                    await _castService.UpdateFileItem(file);
                }

                await _castItHub.Clients.All.PlayListIsBusy(playList.Id, false);
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

                if (isAFolder)
                {
                    await _castService.RemoveFilesThatStartsWith(playlist.Id, oldPath);
                    await _castService.AddFolder(playlist.Id, false, new[] { newPath });
                }
                else
                {
                    await _castService.AddFiles(playlist.Id, new[] { newPath });
                    await _castService.RemoveFiles(playlist.Id, file.Id);
                }
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
    }
}