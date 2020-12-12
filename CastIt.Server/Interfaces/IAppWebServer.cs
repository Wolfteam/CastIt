using CastIt.Application.Interfaces;
using System;
using System.Threading;
using CastIt.Infrastructure.Interfaces;

namespace CastIt.Server.Interfaces
{
    public interface IAppWebServer : IBaseWebServer
    {
        void Init(string previewPath,
            string subtitlesPath,
            IViewForMediaWebSocket view,
            ICastService castService,
            CancellationToken cancellationToken,
            int? startPort = null);
        void Init(string previewPath, string subtitlesPath, ICastService castService,
            CancellationToken cancellationToken, int? startPort = null);
    }
}
