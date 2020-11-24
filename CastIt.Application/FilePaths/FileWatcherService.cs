using CastIt.Application.Common;
using CastIt.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Application.FilePaths
{
    public class FileWatcherService : IFileWatcherService
    {
        private readonly ILogger<FileWatcherService> _logger;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
        public IReadOnlyList<string> PathsToWatch { get; private set; }
        public bool IsListening => _watchers.Any();

        public Func<string, Task> OnFileCreated { get; set; }
        public Func<string, Task> OnFileChanged { get; set; }
        public Func<string, Task> OnFileDeleted { get; set; }
        public Func<string, string, Task> OnFileRenamed { get; set; }

        public FileWatcherService(ILogger<FileWatcherService> logger)
        {
            _logger = logger;
        }

        public void StartListening(IReadOnlyList<string> paths)
        {
            PathsToWatch = paths;
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
                        StopListening(watcher);
                }
            }

            foreach (var path in newPaths)
            {
                if (_watchers.ContainsKey(path))
                    continue;
                WatchPath(path);
            }
        }

        public void StopListening()
        {
            foreach (var (path, watcher) in _watchers)
            {
                _logger.LogInformation($"{nameof(StopListening)}: Stopping the watcher for path = {path}");
                StopListening(watcher);
            }
            _watchers.Clear();
        }

        private void StopListening(FileSystemWatcher watcher)
        {
            _logger.LogInformation($"{nameof(StopListening)}: Stopping watcher for path = {watcher.Path}");
            watcher.Created -= OnChanged;
            watcher.Changed -= OnChanged;
            watcher.Deleted -= OnChanged;
            watcher.Renamed -= OnRenamed;
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        private void WatchPath(string path)
        {
            _logger.LogInformation($"{nameof(WatchPath)}: Starting the watcher for path = {path}");
            if (_watchers.ContainsKey(path))
            {
                StopListening(_watchers[path]);
                _watchers.Remove(path);
            }

            var watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size
            };
            foreach (var filter in FileFormatConstants.AllowedFormats)
            {
                watcher.Filters.Add($"*{filter}");
            }

            watcher.Created += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
            _watchers.Add(path, watcher);
        }

        private async void OnChanged(object source, FileSystemEventArgs e)
        {
            _logger.LogInformation($"{nameof(OnChanged)}: File: " + e.FullPath + " " + e.ChangeType);
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Deleted:
                    if (OnFileDeleted != null)
                        await OnFileDeleted.Invoke(e.FullPath);
                    break;
                case WatcherChangeTypes.Changed:
                    if (OnFileChanged != null)
                        await OnFileChanged.Invoke(e.FullPath);
                    break;
                case WatcherChangeTypes.Created:
                    if (OnFileCreated != null)
                        await OnFileCreated.Invoke(e.FullPath);
                    break;
            }
        }

        private async void OnRenamed(object source, RenamedEventArgs e)
        {
            _logger.LogInformation($"{nameof(OnRenamed)}: File: {e.OldFullPath} renamed to {e.FullPath}");
            if (OnFileRenamed != null)
                await OnFileRenamed.Invoke(e.OldFullPath, e.FullPath);
        }
    }
}
