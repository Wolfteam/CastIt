using CastIt.Domain.Enums;
using CastIt.GoogleCast.Models.Play;
using CastIt.Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.GoogleCast.Interfaces
{
    public interface IPlayMediaRequestGenerator
    {
        string Identifier { get; }

        Task<bool> CanHandleRequest(
            string mrl,
            AppFileType type,
            CancellationToken cancellationToken = default);

        Task<PlayMediaRequest> BuildRequest(
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken = default);

        Task HandleSecondsChanged(
            ServerFileItem file,
            ServerAppSettings settings,
            PlayMediaRequest request,
            double newSeconds,
            CancellationToken cancellationToken = default);
    }
}
