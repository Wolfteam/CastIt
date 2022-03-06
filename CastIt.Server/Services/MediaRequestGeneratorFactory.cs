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

        Task HandleSecondsChanged(
            ServerFileItem file,
            ServerAppSettings settings,
            PlayMediaRequest request,
            double newSeconds,
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
        //TODO: MAYBE CACHE THE CAN HANDLE REQUEST ?
        public async Task<PlayMediaRequest> BuildRequest(
            ServerFileItem file,
            ServerAppSettings settings,
            double seekSeconds,
            bool fileOptionsChanged,
            CancellationToken cancellationToken = default)
        {
            var impl = await GetImplementation(file.Path, file.Type, cancellationToken);
            return await impl.BuildRequest(file, settings, seekSeconds, fileOptionsChanged, cancellationToken);
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

        public async Task HandleSecondsChanged(ServerFileItem file,
            ServerAppSettings settings,
            PlayMediaRequest request,
            double newSeconds,
            CancellationToken cancellationToken = default)
        {
            var impl = await GetImplementation(file.Path, file.Type, cancellationToken);
            await impl.HandleSecondsChanged(file, settings, request, newSeconds, cancellationToken);
        }

        private async Task<IPlayMediaRequestGenerator> GetImplementation(
            string path,
            AppFileType type,
            CancellationToken cancellationToken)
        {
            foreach (var (_, impl) in _implementations)
            {
                if (await impl.CanHandleRequest(path, type, cancellationToken))
                {
                    return impl;
                }
            }
            throw new InvalidRequestException($"No implementation was found that can handle = {path}");
        }
    }
}