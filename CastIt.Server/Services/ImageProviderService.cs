using CastIt.Server.Interfaces;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public class ImageProviderService : IImageProviderService
    {
        private const string NoImage = "NoImg.png";

        private readonly IFileService _fileService;
        private readonly IServerService _serverService;
        private readonly string _imagesBasePath;
        private readonly string _noImgFoundPath;

        public byte[] NoImageBytes { get; private set; }
        public byte[] TransparentImageBytes { get; } = new List<byte>
        {
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A,
            0x00,
            0x00,
            0x00,
            0x0D,
            0x49,
            0x48,
            0x44,
            0x52,
            0x00,
            0x00,
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x01,
            0x08,
            0x06,
            0x00,
            0x00,
            0x00,
            0x1F,
            0x15,
            0xC4,
            0x89,
            0x00,
            0x00,
            0x00,
            0x0A,
            0x49,
            0x44,
            0x41,
            0x54,
            0x78,
            0x9C,
            0x63,
            0x00,
            0x01,
            0x00,
            0x00,
            0x05,
            0x00,
            0x01,
            0x0D,
            0x0A,
            0x2D,
            0xB4,
            0x00,
            0x00,
            0x00,
            0x00,
            0x49,
            0x45,
            0x4E,
            0x44,
            0xAE
        }.ToArray();

        public ImageProviderService(
            IFileService fileService,
            IServerService serverService,
            IWebHostEnvironment environment)
        {
            _fileService = fileService;
            _serverService = serverService;
            _imagesBasePath = $"{environment.WebRootPath}/Images";
            _noImgFoundPath = $"{_imagesBasePath}/{NoImage}";
        }

        public async Task Init()
        {
            string path = GetNoImagePath();
            NoImageBytes = await File.ReadAllBytesAsync(path);
        }

        public string GetPlayListImageUrl(ServerPlayList playList, ServerFileItem currentPlayedFile)
        {
            if (playList == null)
                throw new ArgumentNullException("The playlist cannot be null");

            //If the current played file belongs to this playlist
            if (currentPlayedFile?.PlayListId == playList.Id)
            {
                //Url file images are downloaded to disk
                return currentPlayedFile.IsUrlFile
                    ? GetPlayListImageUrl(currentPlayedFile.Id, _fileService.GetTemporalPreviewImagePath(currentPlayedFile.Id), true)
                    : GetPlayListImageUrl(currentPlayedFile.Id, currentPlayedFile.Path, currentPlayedFile.IsLocalFile);
            }

            //If the item is not in the playlist, then try to return an image that is part of the playlist
            return GetPlayListImageUrl(playList);
        }

        public string GetPlayListImageUrl(ServerPlayList playList)
        {
            var selected = playList.Files
                .Select(f => new
                {
                    Id = f.Id,
                    IsLocal = f.IsLocalFile,
                    Thumbnail = f.IsUrlFile
                        ? _fileService.GetTemporalPreviewImagePath(f.Id)
                        : _fileService.GetFirstThumbnailFilePath(f.Id),
                })
                .FirstOrDefault(f => _fileService.Exists(f.Thumbnail));

            return GetPlayListImageUrl(selected?.Id, selected?.Thumbnail, selected?.IsLocal ?? false);
        }

        public bool IsNoImage(string filename)
            => filename == NoImage;

        public string GetImagesPath()
            => _imagesBasePath;

        public string GetNoImagePath()
            => _noImgFoundPath;

        private string GetPlayListImageUrl(long? id, string path, bool isLocal)
        {
            if (!id.HasValue || string.IsNullOrWhiteSpace(path))
            {
                return _serverService.GetChromeCastPreviewUrl(-1);
            }

            return _fileService.IsUrlFile(path) && !isLocal
                ? path
                : _serverService.GetChromeCastPreviewUrl(id.Value);
        }
    }
}
