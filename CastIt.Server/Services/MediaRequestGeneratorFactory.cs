using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.GoogleCast.Interfaces;
using CastIt.GoogleCast.Models.Play;
using CastIt.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public interface IMediaRequestGeneratorFactory
    {
        void Add(IPlayMediaRequestGenerator impl);

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
    }

    public class MediaRequestGeneratorFactory : IMediaRequestGeneratorFactory
    {
        private readonly Dictionary<string, IPlayMediaRequestGenerator> _implementations
            = new Dictionary<string, IPlayMediaRequestGenerator>();

        public void Add(IPlayMediaRequestGenerator impl)
        {
            _implementations.TryAdd(impl.Identifier, impl);
        }

        public async Task<PlayMediaRequest> BuildRequest(
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken = default)
        {
            foreach (var (_, impl) in _implementations)
            {
                if (await impl.CanHandleRequest(file.Path, file.Type, cancellationToken))
                {
                    return await impl.BuildRequest(file, settings, seekSeconds, fileOptionsChanged, cancellationToken);
                }
            }

            throw new InvalidRequestException($"No implementation was found that can handle = {file.Path}");
        }

        public async Task<bool> CanHandleRequest(
            string mrl,
            AppFileType type,
            CancellationToken cancellationToken = default)
        {
            foreach (var (_, value) in _implementations)
            {
                bool can = await value.CanHandleRequest(mrl, type, cancellationToken);
                if (can)
                {
                    return true;
                }
            }

            return false;
        }
    }
}