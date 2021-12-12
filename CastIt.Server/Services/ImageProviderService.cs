using CastIt.Server.Interfaces;
using CastIt.Shared.FilePaths;
using CastIt.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace CastIt.Server.Services
{
    public class ImageProviderService : IImageProviderService
    {
        private const string NoImage = "NoImg.png";

        private readonly IFileService _fileService;
        private readonly IServerService _serverService;
        private readonly string _imagesBasePath;
        private readonly string _noImgFoundPath;

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

        public string GetPlayListImageUrl(ServerPlayList playList, ServerFileItem currentPlayedFile)
        {
            if (playList == null)
                throw new ArgumentNullException("The playlist cannot be null");

            //If the current played file belongs to this playlist
            if (currentPlayedFile?.PlayListId == playList.Id)
            {
                //Url file images are downloaded to disk
                return currentPlayedFile.IsUrlFile
                    ? GetPlayListImageUrl(_fileService.GetTemporalPreviewImagePath(currentPlayedFile.Id), true)
                    : GetPlayListImageUrl(currentPlayedFile.Path, currentPlayedFile.IsLocalFile);
            }

            //If the item is not in the playlist, then try to return an image that is part of the playlist
            return GetPlayListImageUrl(playList);
        }

        public string GetPlayListImageUrl(ServerPlayList playList)
        {
            var selected = playList.Files
                .Select(f => new
                {
                    IsLocal = f.IsLocalFile,
                    Thumbnail = f.IsUrlFile
                        ? _fileService.GetTemporalPreviewImagePath(f.Id)
                        : _fileService.GetFirstThumbnailFilePath(f.Name),
                })
                .FirstOrDefault(f => _fileService.Exists(f.Thumbnail));
            return GetPlayListImageUrl(selected?.Thumbnail, selected?.IsLocal ?? false);
        }

        public bool IsNoImage(string filename)
            => filename == NoImage;

        public string GetImagesPath()
            => _imagesBasePath;

        public string GetNoImagePath()
            => _noImgFoundPath;

        private string GetPlayListImageUrl(string path, bool isLocal)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return _serverService.GetChromeCastPreviewUrl(GetNoImagePath());
            }

            return _fileService.IsUrlFile(path) && !isLocal
                ? path
                : _serverService.GetChromeCastPreviewUrl(path);
        }
    }
}
