using CastIt.Domain;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Utils;
using CastIt.Server.Common.Comparers;
using CastIt.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public partial class ServerCastService
    {
        public Task<List<GetAllPlayListResponseDto>> GetAllPlayLists()
        {
            RefreshPlayListImages();
            var mapped = _mapper.Map<List<GetAllPlayListResponseDto>>(PlayLists.OrderBy(pl => pl.Position));
            return Task.FromResult(mapped);
        }

        public PlayListItemResponseDto GetPlayList(long playListId)
        {
            var playList = GetPlayListInternal(playListId);
            RefreshPlayListImage(playList);
            var mapped = _mapper.Map<PlayListItemResponseDto>(playList);
            return mapped;
        }

        public PlayListItemResponseDto GetPlayList(string name)
        {
            var playList = PlayLists.Find(pl => pl.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(GetPlayListInternal)}: A playlist with name = {name} doesn't exists");
                _server.OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
                throw new PlayListNotFoundException($"A playlist with name = {name} does not exist");
            }
            RefreshPlayListImage(playList);
            var mapped = _mapper.Map<PlayListItemResponseDto>(playList);
            return mapped;
        }

        public async Task<PlayListItemResponseDto> AddNewPlayList()
        {
            var position = PlayLists.Any()
                ? PlayLists.Max(pl => pl.Position) + 1
                : 1;
            var playList = await _appDataService.AddNewPlayList($"New PlayList {PlayLists.Count}", position);
            var serverPlayList = _mapper.Map<ServerPlayList>(playList);
            serverPlayList.ImageUrl = _imageProviderService.GetPlayListImageUrl(serverPlayList, CurrentPlayedFile);

            PlayLists.Add(serverPlayList);

            SendPlayListAdded(_mapper.Map<GetAllPlayListResponseDto>(serverPlayList));
            return _mapper.Map<PlayListItemResponseDto>(serverPlayList);
        }

        public async Task DeletePlayList(long playListId)
        {
            await _appDataService.DeletePlayList(playListId);
            PlayLists.RemoveAll(pl => pl.Id == playListId);
            SendPlayListDeleted(playListId);
            //InitializeOrUpdateFileWatcher(true);
        }

        public async Task DeleteAllPlayLists(long exceptId = -1)
        {
            if (PlayLists.Count <= 1)
                return;
            var ids = PlayLists
                .Where(pl => pl.Id != exceptId)
                .Select(pl => pl.Id)
                .ToList();
            await _appDataService.DeletePlayLists(ids);

            PlayLists.RemoveAll(pl => ids.Contains(pl.Id));
            foreach (var id in ids)
            {
                SendPlayListDeleted(id);
            }

            //InitializeOrUpdateFileWatcher(true);
        }

        public async Task UpdatePlayList(long playListId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new InvalidRequestException($"The provided name for playlistId = {playListId} cannot be null");
            }

            var playList = GetPlayListInternal(playListId);
            newName = newName.Trim();
            if (newName.Length > 100)
            {
                newName = newName.Substring(0, 100);
            }

            playList.Name = newName;

            await _appDataService.UpdatePlayList(playListId, newName, playList.Position);
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public void UpdatePlayListPosition(long playListId, int newIndex)
        {
            var playList = GetPlayListInternal(playListId, false);
            if (playList == null || PlayLists.Count - 1 < newIndex)
            {
                _logger.LogWarning($"{nameof(UpdatePlayListPosition)}: PlaylistId = {playListId} doesn't exist or the newIndex = {newIndex} is not valid");
                return;
            }
            var currentIndex = PlayLists.IndexOf(playList);

            _logger.LogInformation($"{nameof(UpdatePlayListPosition)}: Moving playlist from index = {currentIndex} to newIndex = {newIndex}");
            PlayLists.RemoveAt(currentIndex);
            PlayLists.Insert(newIndex, playList);

            for (int i = 0; i < PlayLists.Count; i++)
            {
                var item = PlayLists[i];
                item.Position = i;
            }
            SendPlayListsChanged(_mapper.Map<List<GetAllPlayListResponseDto>>(PlayLists));
        }

        public Task AddFolder(long playListId, bool includeSubFolders, string[] folders)
        {
            var files = new List<string>();
            foreach (var folder in folders)
            {
                _logger.LogInformation($"{nameof(AddFolder)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(s => FileFormatConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToList();
                files.AddRange(filesInDir);
            }
            return AddFiles(playListId, files.ToArray());
        }

        public async Task AddFiles(long playListId, string[] paths)
        {
            _logger.LogInformation($"{nameof(AddFiles)}: Trying to add new files to playListId = {playListId}...");
            if (paths == null || paths.Length == 0)
            {
                _logger.LogInformation($"{nameof(AddFiles)}: No paths were provided");
                throw new InvalidRequestException("No paths were provided", AppMessageType.NoFilesToBeAdded);
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > 1000))
            {
                _logger.LogInformation($"{nameof(AddFiles)}: One of the provided paths is not valid");
                throw new FileNotSupportedException("One of the provided paths is not valid", AppMessageType.FilesAreNotValid);
            }

            var playList = PlayLists.Find(pl => pl.Id == playListId);
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(AddFiles)}: PlayListId = {playListId} does not exist");
                throw new PlayListNotFoundException($"PlayListId = {playListId} does not exist");
            }

            int startIndex = playList.Files.Count + 1;
            List<FileItem> files = paths.Where(path =>
                {
                    string filename = Path.GetFileName(path);
                    string ext = Path.GetExtension(path);
                    return !filename.StartsWith('.') &&
                           FileFormatConstants.AllowedFormats.Contains(ext, StringComparer.OrdinalIgnoreCase) &&
                           playList.Files.All(f => f.Path != path);
                })
                .OrderBy(p => p, new WindowsExplorerComparer())
                .Select((path, index) => new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = playListId,
                    Path = path,
                    CreatedAt = DateTime.Now
                })
                .ToList();

            if (files.Count == 0)
            {
                return;
            }

            var createdFiles = await _appDataService.AddFiles(files);
            _server.OnFilesAdded.Invoke(playListId, createdFiles.ToArray());
        }

        public async Task AddFile(long playListId, FileItem file)
        {
            var playList = PlayLists.Find(f => f.Id == playListId);
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(AddFile)}: PlayListId = {playListId} does not exist. It may have been removed");
                return;
            }

            var serverFileItem = ServerFileItem.From(_fileService, file);
            await UpdateFileItem(serverFileItem);
            playList.Files.Add(serverFileItem);
            SendFileAdded(_mapper.Map<FileItemResponseDto>(serverFileItem));
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public async Task AddUrl(long playListId, string url, bool onlyVideo)
        {
            if (!NetworkUtils.IsInternetAvailable())
            {
                var msg = $"Can't add file = {url} to playListId = {playListId} cause there's no internet connection";
                _logger.LogInformation($"{nameof(AddUrl)}: {msg}");
                throw new InvalidRequestException(msg, AppMessageType.NoInternetConnection);
            }

            bool isUrlFile = _fileService.IsUrlFile(url);
            if (!isUrlFile || !_youtubeUrlDecoder.IsYoutubeUrl(url))
            {
                var msg = $"Url = {url} is not supported";
                _logger.LogInformation($"{nameof(AddUrl)}: {msg}");
                throw new FileNotSupportedException(msg, AppMessageType.UrlNotSupported);
            }

            try
            {
                SendPlayListBusy(playListId, true);
                _logger.LogInformation($"{nameof(AddUrl)}: Trying to parse url = {url}");
                if (!_youtubeUrlDecoder.IsPlayList(url) || _youtubeUrlDecoder.IsPlayListAndVideo(url) && onlyVideo)
                {
                    _logger.LogInformation(
                        $"{nameof(AddUrl)}: Url is either not a playlist or we just want the video, parsing it...");
                    await AddYoutubeUrl(playListId, url);
                    return;
                }

                _logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist...");
                var links = await _youtubeUrlDecoder.ParsePlayList(url, FileCancellationTokenSource.Token);
                foreach (var link in links)
                {
                    if (FileCancellationTokenSource.IsCancellationRequested)
                        break;
                    _logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist url = {link}");
                    await AddYoutubeUrl(playListId, link);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(AddUrl)}: Couldn't parse url = {url}");
                _telemetry.TrackError(e);
                throw;
            }
            finally
            {
                SendPlayListBusy(playListId, false);
            }
        }

        public Task AddFolderOrFileOrUrl(long playListId, string path, bool includeSubFolders, bool onlyVideo)
        {
            _logger.LogInformation($"{nameof(AddFolderOrFileOrUrl)}: Trying to add path = {path} to playListId = {playListId}...");
            if (Directory.Exists(path))
            {
                _logger.LogInformation($"{nameof(AddFolderOrFileOrUrl)}: The provided path is a directory....");
                return AddFolder(playListId, includeSubFolders, new[] { path });
            }

            if (File.Exists(path))
            {
                _logger.LogInformation($"{nameof(AddFolderOrFileOrUrl)}: The provided path is a file....");
                return AddFiles(playListId, new[] { path });
            }

            if (Uri.TryCreate(path, UriKind.Absolute, out _))
            {
                _logger.LogInformation($"{nameof(AddFolderOrFileOrUrl)}: The provided path is a url....");
                return AddUrl(playListId, path, onlyVideo);
            }

            _logger.LogWarning($"The provided path = {path} is not valid");
            throw new InvalidRequestException("The provided path is not valid");
        }

        private async Task AddYoutubeUrl(long playListId, string url)
        {
            var media = await _youtubeUrlDecoder.ParseBasicInfo(url);
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > AppWebServerConstants.MaxCharsPerString)
            {
                media.Title = media.Title.Substring(0, AppWebServerConstants.MaxCharsPerString);
            }
            var playList = GetPlayListInternal(playListId);
            var createdFile = await _appDataService.AddFile(playListId, url, playList.Files.Count + 1, media.Title);
            var serverFileItem = ServerFileItem.From(_fileService, createdFile);
            await UpdateFileItem(serverFileItem);
            playList.Files.Add(serverFileItem);
            SendFileAdded(_mapper.Map<FileItemResponseDto>(serverFileItem));
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public void SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var playlist = GetPlayListInternal(id);
            if (playlist.Loop == loop && playlist.Shuffle == shuffle)
            {
                return;
            }
            playlist.Loop = loop;
            playlist.Shuffle = shuffle;
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playlist));
        }

        public void RefreshPlayListImages()
        {
            foreach (var playList in PlayLists)
            {
                RefreshPlayListImage(playList);
            }
        }

        public void RefreshPlayListImage(ServerPlayList playList)
        {
            string imageUrl = _imageProviderService.GetPlayListImageUrl(playList, CurrentPlayedFile);
            bool changed = playList.ImageUrl != imageUrl;
            playList.ImageUrl = imageUrl;
            if (changed)
                SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
        }

        public void ExchangeLastFilePosition(long playListId, long toFileId)
        {
            var playList = GetPlayListInternal(playListId, false);
            ExchangeLastFilePosition(playList, toFileId);
        }

        public void ExchangeLastFilePosition(ServerPlayList playList, long toFileId)
        {
            var newFile = playList?.Files.LastOrDefault();
            if (newFile == null)
                return;
            ExchangeFilePosition(playList, newFile.Id, toFileId);
        }

        public void ExchangeFilePosition(long playListId, long fromFileId, long toFileId)
        {
            var playList = GetPlayListInternal(playListId, false);
            ExchangeFilePosition(playList, fromFileId, toFileId);
        }

        public void ExchangeFilePosition(ServerPlayList playList, long fromFileId, long toFileId)
        {
            var fromFile = playList?.Files.Find(f => f.Id == fromFileId);
            var toFile = playList?.Files.Find(f => f.Id == toFileId);
            if (fromFile == null || toFile == null)
            {
                return;
            }

            var toIndex = playList.Files.IndexOf(toFile);
            UpdateFilePosition(playList.Id, fromFileId, toIndex);
        }

        private ServerPlayList GetPlayListInternal(long id, bool throwOnNotFound = true)
        {
            var playList = PlayLists.Find(pl => pl.Id == id);
            if (playList != null)
                return playList;
            _logger.LogWarning($"{nameof(GetPlayListInternal)}: PlaylistId = {id} doesn't exists");
            _server.OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
            if (throwOnNotFound)
                throw new PlayListNotFoundException($"PlayListId = {id} does not exist");
            return null;
        }
    }
}
