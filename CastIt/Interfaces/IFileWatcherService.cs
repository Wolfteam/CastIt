using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Interfaces
{
    public interface IFileWatcherService
    {
        IReadOnlyList<string> PathsToWatch { get; }
        bool IsListening { get; }
        Func<string, Task> OnFileCreated { get; set; }
        Func<string, Task> OnFileChanged { get; set; }
        Func<string, Task> OnFileDeleted { get; set; }
        Func<string, string, Task> OnFileRenamed { get; set; }

        void StartListening();

        void StartListening(IReadOnlyList<string> paths);

        void UpdateWatchers(IReadOnlyList<string> newPaths, bool deleteOlderWatchers = true);

        void StopListening();
    }
}
