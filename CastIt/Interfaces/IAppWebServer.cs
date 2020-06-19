using System;
using System.Collections.Generic;
using System.Threading;

namespace CastIt.Interfaces
{
    public interface IAppWebServer : IDisposable
    {
        static IReadOnlyList<string> AllowedQueryParameters { get; }

        void Init(CancellationToken cancellationToken);

        string GetMediaUrl(string filePath, int videoStreamIndex, int audioStreamIndex, double seconds);

        string GetPreviewPath(string filepath);

        string GetSubTitlePath(string filepath);
    }
}
