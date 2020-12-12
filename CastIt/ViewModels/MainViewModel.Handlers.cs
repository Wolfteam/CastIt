using CastIt.Application.Common;
using CastIt.ViewModels.Items;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.ViewModels
{
    public partial class MainViewModel
    {
        private void InitializeCastHandlers()
        {
            Logger.LogInformation($"{nameof(InitializeCastHandlers)}: Setting cast events...");
            _castService.OnFileLoaded += OnFileLoaded;
            _castService.OnTimeChanged += OnFileDurationChanged;
            _castService.OnPositionChanged += OnFilePositionChanged;
            _castService.OnEndReached += OnFileEndReached;
            _castService.QualitiesChanged += OnQualitiesChanged;
            _castService.OnPaused += OnPaused;
            _castService.OnDisconnected += OnDisconnected;
            _castService.GetSubTitles = () => CurrentFileSubTitles.FirstOrDefault(f => f.IsSelected)?.Path;
            _castService.OnVolumeChanged += OnVolumeChanged;
            _castService.OnFileLoadFailed += OnFileLoadFailed;
        }

        private void RemoveCastHandlers()
        {
            Logger.LogInformation($"{nameof(RemoveCastHandlers)}: Removing cast events...");
            _castService.OnFileLoaded -= OnFileLoaded;
            _castService.OnTimeChanged -= OnFileDurationChanged;
            _castService.OnPositionChanged -= OnFilePositionChanged;
            _castService.OnEndReached -= OnFileEndReached;
            _castService.QualitiesChanged -= OnQualitiesChanged;
            _castService.OnPaused -= OnPaused;
            _castService.OnDisconnected -= OnDisconnected;
            _castService.OnVolumeChanged -= OnVolumeChanged;
            _castService.OnFileLoadFailed -= OnFileLoadFailed;
        }

        private void InitializeOrUpdateFileWatcher(bool update)
        {
            Logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Getting directories to watch...");
            var dirs = PlayLists.SelectMany(pl => pl.Items)
                .Where(f => f.IsLocalFile)
                .Select(f => Path.GetDirectoryName(f.Path))
                .Distinct()
                .ToList();

            Logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Got = {dirs.Count} directories...");
            if (!update)
            {
                Logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Starting to watch for {dirs.Count} directories...");
                _fileWatcherService.StartListening(dirs);
                _fileWatcherService.OnFileCreated = OnFwCreated;
                _fileWatcherService.OnFileChanged = OnFwChanged;
                _fileWatcherService.OnFileDeleted = OnFwDeleted;
                _fileWatcherService.OnFileRenamed = OnFwRenamed;
            }
            else
            {
                Logger.LogInformation($"{nameof(InitializeOrUpdateFileWatcher)}: Updating watched directories...");
                _fileWatcherService.UpdateWatchers(dirs);
            }
        }

        #region Cast handlers
        private async void OnFileLoaded(
            string title,
            string thumbUrl,
            double duration,
            double volumeLevel,
            bool isMuted)
        {
            CurrentFileThumbnail = thumbUrl;
            VolumeLevel = volumeLevel;
            IsMuted = isMuted;
            if (_currentlyPlayedFile?.IsUrlFile == true)
            {
                CurrentlyPlayingFilename = title;
                _currentlyPlayedFile.Name = title;
                await _currentlyPlayedFile.SetDuration(duration);
                await RaisePropertyChanged(() => CurrentFileDuration);
            }

            _appWebServer.OnFileLoaded?.Invoke();
        }

        private void OnFileDurationChanged(double seconds)
        {
            IsPaused = false;

            if (_currentlyPlayedFile is null)
                return;

            CurrentPlayedSeconds = seconds;

            var elapsed = TimeSpan.FromSeconds(seconds)
                .ToString(FileFormatConstants.FullElapsedTimeFormat);
            var total = TimeSpan.FromSeconds(_currentlyPlayedFile.TotalSeconds)
                .ToString(FileFormatConstants.FullElapsedTimeFormat);
            if (_currentlyPlayedFile.IsUrlFile && _currentlyPlayedFile.TotalSeconds <= 0)
                ElapsedTimeString = $"{elapsed}";
            else
                ElapsedTimeString = $"{elapsed} / {total}";

            var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
            playlist?.UpdatePlayedTime();
        }

        private void OnFilePositionChanged(double playedPercentage)
            => PlayedPercentage = playedPercentage;

        private void OnFileEndReached()
        {
            Logger.LogInformation($"{nameof(OnFileEndReached)}: End reached for file = {_currentlyPlayedFile?.Path}");

            SetCurrentlyPlayingInfo(null, false);

            IsPaused = false;

            if (_currentlyPlayedFile != null)
            {
                var playlist = PlayLists.FirstOrDefault(pl => pl.Id == _currentlyPlayedFile.PlayListId);
                playlist?.UpdatePlayedTime();
            }

            if (_currentlyPlayedFile?.Loop == true)
            {
                Logger.LogInformation($"{nameof(OnFileEndReached)}: Looping file = {_currentlyPlayedFile?.Path}");
                _currentlyPlayedFile.PlayedPercentage = 0;
                _currentlyPlayedFile.PlayCommand.Execute();
                return;
            }

            if (_settingsService.PlayNextFileAutomatically)
            {
                Logger.LogInformation($"{nameof(OnFileEndReached)}: Play next file is enabled. Playing the next file...");
                GoTo(true, true);
            }
            else
            {
                Logger.LogInformation($"{nameof(OnFileEndReached)}: Play next file is disabled. Next file won't be played, playback will stop now");
                StopPlayBackCommand.Execute();
            }
        }

        private void OnQualitiesChanged(int selectedQuality, List<int> qualities)
        {
            var vms = qualities.OrderBy(q => q).Select(q => new FileItemOptionsViewModel
            {
                Id = q,
                IsSelected = selectedQuality == q,
                IsEnabled = qualities.Count > 1,
                IsQuality = true,
                Text = $"{q}"
            }).ToList();
            CurrentFileQualities.ReplaceWith(vms);
        }

        private void OnPaused()
            => IsPaused = true;

        private void OnDisconnected()
            => OnStoppedPlayBack();

        private void OnVolumeChanged(double level, bool isMuted)
        {
            VolumeLevel = level;
            IsMuted = isMuted;
        }

        private async void OnFileLoadFailed()
        {
            _appWebServer.OnFileLoadingError?.Invoke(GetText("CouldntPlayFile"));
            await StopPlayBack();
            await ShowSnackbarMsg(GetText("CouldntPlayFile"));
        }
        #endregion

        #region FW handlers
        private Task OnFwCreated(string path, bool isAFolder)
        {
            return OnFwChanged(path, isAFolder);
        }

        private async Task OnFwChanged(string path, bool isAFolder)
        {
            var files = PlayLists
                .SelectMany(f => f.Items)
                .Where(f => isAFolder ? f.Path.StartsWith(path) : f.Path == path)
                .ToList();
            foreach (var file in files)
            {
                var playlist = PlayLists.FirstOrDefault(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;

                await playlist.SetFileInfo(file.Id, _setDurationTokenSource.Token);
                _appWebServer?.OnFileChanged(playlist.Id);
            }
        }

        private Task OnFwDeleted(string path, bool isAFolder)
        {
            return OnFwChanged(path, isAFolder);
        }

        private async Task OnFwRenamed(string oldPath, string newPath, bool isAFolder)
        {
            var files = PlayLists
                .SelectMany(f => f.Items)
                .Where(f => isAFolder ? f.Path.StartsWith(oldPath) : f.Path == oldPath)
                .ToList();
            foreach (var file in files)
            {
                var playlist = PlayLists.FirstOrDefault(f => f.Id == file.PlayListId);
                if (playlist == null)
                    continue;

                if (isAFolder)
                {
                    //Here I'm not sure how to retain the order
                    await playlist.RemoveFilesThatStartsWith(oldPath);
                    await playlist.OnFolderAddedCommand.ExecuteAsync(new[] { newPath });
                }
                else
                {
                    await playlist.OnFilesAddedCommand.ExecuteAsync(new[] { newPath });
                    playlist.ExchangeLastFilePosition(file.Id);
                    await playlist.RemoveFile(file.Id);
                }
                _appWebServer?.OnPlayListChanged(playlist.Id);
            }
        }
        #endregion
    }
}
