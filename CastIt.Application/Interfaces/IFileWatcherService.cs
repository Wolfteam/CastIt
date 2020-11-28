using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Application.Interfaces
{
    public interface IFileWatcherService
    {
        IReadOnlyList<string> PathsToWatch { get; }
        bool IsListening { get; }
        Func<string, bool, Task> OnFileCreated { get; set; }
        Func<string, bool, Task> OnFileChanged { get; set; }
        Func<string, bool, Task> OnFileDeleted { get; set; }
        Func<string, string, bool, Task> OnFileRenamed { get; set; }

        void StartListening();

        void StartListening(IReadOnlyList<string> paths);

        void UpdateWatchers(IReadOnlyList<string> newPaths, bool deleteOlderWatchers = true);

        void StopListening();
    }
}
