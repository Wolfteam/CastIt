using CastIt.Domain;
using CastIt.Server.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.FileWatcher
{
    public class FileWatcherService : IFileWatcherService
    {
        private readonly ILogger<FileWatcherService> _logger;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
        private readonly Dictionary<string, FileSystemWatcher> _dirWatchers = new Dictionary<string, FileSystemWatcher>();
        public IReadOnlyList<string> PathsToWatch { get; private set; }
        public bool IsListening => _watchers.Any() || _dirWatchers.Any();

        public Func<string, bool, Task> OnFileCreated { get; set; }
        public Func<string, bool, Task> OnFileChanged { get; set; }
        public Func<string, bool, Task> OnFileDeleted { get; set; }
        public Func<string, string, bool, Task> OnFileRenamed { get; set; }

        public FileWatcherService(ILogger<FileWatcherService> logger)
        {
            _logger = logger;
        }

        public void StartListening(IReadOnlyList<string> paths)
        {
            PathsToWatch = paths.Distinct().ToList();
            StartListening();
        }

        public void StartListening()
        {
            if (!PathsToWatch.Any())
                return;

            StopListening();

            foreach (var path in PathsToWatch)
            {
                if (!Directory.Exists(path))
                    continue;
                WatchPath(path);
            }
        }

        public void UpdateWatchers(IReadOnlyList<string> newPaths, bool deleteOlderWatchers = true)
        {
            if (deleteOlderWatchers)
            {
                foreach (var (path, watcher) in _watchers)
                {
                    if (!newPaths.Contains(path))
                        StopListening(watcher, false);
                }

                foreach (var (path, watcher) in _dirWatchers)
                {
                    if (!newPaths.Contains(path))
                        StopListening(watcher, true);
                }
            }

            foreach (var path in newPaths)
            {
                if (!_watchers.ContainsKey(path))
                    WatchPath(path);
            }
        }

        public void StopListening()
        {
            foreach (var (path, watcher) in _watchers)
            {
                _logger.LogInformation($"{nameof(StopListening)}: Stopping the watcher for path = {path}");
                StopListening(watcher, false);
            }

            foreach (var (path, watcher) in _dirWatchers)
            {
                _logger.LogInformation($"{nameof(StopListening)}: Stopping the watcher for dir path = {path}");
                StopListening(watcher, true);
            }
            _watchers.Clear();
            _dirWatchers.Clear();
        }

        private void StopListening(FileSystemWatcher watcher, bool isAFolder)
        {
            _logger.LogInformation($"{nameof(StopListening)}: Stopping watcher for path = {watcher.Path}");
            watcher.Created -= OnChanged;
            watcher.Changed -= OnChanged;
            watcher.Deleted -= OnChanged;
            watcher.Renamed -= OnRenamed;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            if (isAFolder)
                _dirWatchers.Remove(watcher.Path);
            else
                _watchers.Remove(watcher.Path);
        }

        private void WatchPath(string path)
        {
            _logger.LogInformation($"{nameof(WatchPath)}: Starting the watcher for path = {path}");
            if (_watchers.ContainsKey(path))
            {
                StopListening(_watchers[path], false);
                _watchers.Remove(path);
            }

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.FileName |
                               NotifyFilters.DirectoryName |
                               NotifyFilters.Size |
                               NotifyFilters.CreationTime |
                               NotifyFilters.Attributes
            };
            foreach (var filter in FileFormatConstants.AllowedFormats)
            {
                watcher.Filters.Add($"*{filter}");
            }
            watcher.Error += OnError;
            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
            _watchers.Add(path, watcher);
            WatchFolder(path);
        }

        private void WatchFolder(string path)
        {
            _logger.LogInformation($"{nameof(WatchFolder)}: Starting the watcher for dir path = {path}");
            if (_dirWatchers.ContainsKey(path))
            {
                StopListening(_dirWatchers[path], true);
                _dirWatchers.Remove(path);
            }
            //TODO: NOT WORKING..
            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.DirectoryName
            };
            watcher.Error += OnError;
            watcher.Created += OnFolderChanged;
            watcher.Changed += OnFolderChanged;
            watcher.Deleted += OnFolderChanged;
            watcher.Renamed += OnFolderRenamed;
            watcher.EnableRaisingEvents = true;
            _dirWatchers.Add(path, watcher);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "Something went wrong while listening for changes");
        }

        private async void OnChanged(object source, FileSystemEventArgs e)
        {
            await OnChanged(e.FullPath, e.ChangeType, false);
        }

        private async void OnRenamed(object source, RenamedEventArgs e)
        {
            await OnRenamed(e.OldFullPath, e.FullPath, false);
        }

        private async void OnFolderChanged(object source, FileSystemEventArgs e)
        {
            await OnChanged(e.FullPath, e.ChangeType, true);
        }

        private async void OnFolderRenamed(object source, RenamedEventArgs e)
        {
            await OnRenamed(e.OldFullPath, e.FullPath, true);
        }

        private async Task OnChanged(string fullPath, WatcherChangeTypes changeType, bool isAFolder)
        {
            _logger.LogInformation($"{nameof(OnChanged)}: Handling change = {changeType} for = {fullPath} which is a folder = {isAFolder}");
            switch (changeType)
            {
                case WatcherChangeTypes.Deleted:
                    if (OnFileDeleted != null)
                        await OnFileDeleted.Invoke(fullPath, isAFolder);
                    break;
                case WatcherChangeTypes.Changed:
                    if (OnFileChanged != null)
                        await OnFileChanged.Invoke(fullPath, isAFolder);
                    break;
                case WatcherChangeTypes.Created:
                    if (OnFileCreated != null)
                        await OnFileCreated.Invoke(fullPath, isAFolder);
                    break;
            }
        }

        private async Task OnRenamed(string oldPath, string newPath, bool isAFolder)
        {
            _logger.LogInformation($"{nameof(OnRenamed)}: Handling rename for = {oldPath} which was renamed to = {newPath} and it is a folder = {isAFolder}");
            if (OnFileRenamed != null)
                await OnFileRenamed.Invoke(oldPath, newPath, isAFolder);
        }
    }
}
