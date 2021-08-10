using CastIt.Application.Common;
using CastIt.Application.Common.Utils;
using CastIt.Application.Server;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Infrastructure.Models;
using CastIt.Server.Common.Comparers;
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

        public async Task<PlayListItemResponseDto> AddNewPlayList()
        {
            var position = PlayLists.Any()
                ? PlayLists.Max(pl => pl.Position) + 1
                : 1;
            var playList = await _appDataService.AddNewPlayList($"New PlayList {PlayLists.Count}", position);
            var serverPlayList = _mapper.Map<ServerPlayList>(playList);
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
                Logger.LogWarning($"{nameof(UpdatePlayListPosition)}: PlaylistId = {playListId} doesn't exist or the newIndex = {newIndex} is not valid");
                return;
            }
            var currentIndex = PlayLists.IndexOf(playList);

            Logger.LogInformation($"{nameof(UpdatePlayListPosition)}: Moving playlist from index = {currentIndex} to newIndex = {newIndex}");
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
                Logger.LogInformation($"{nameof(AddFolder)}: Getting all the media files from folder = {folder}");
                var filesInDir = Directory.EnumerateFiles(folder, "*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(s => FileFormatConstants.AllowedFormats.Contains(Path.GetExtension(s).ToLower()))
                    .ToList();
                files.AddRange(filesInDir);
            }
            return AddFiles(playListId, files.ToArray());
        }

        public async Task AddFiles(long playListId, string[] paths)
        {
            Logger.LogInformation($"{nameof(AddFiles)}: Trying to add new files to playListId = {playListId}...");
            if (paths == null || paths.Length == 0)
            {
                Logger.LogInformation($"{nameof(AddFiles)}: No paths were provided");
                throw new InvalidRequestException("No paths were provided", AppMessageType.NoFilesToBeAdded);
            }

            if (paths.Any(p => string.IsNullOrEmpty(p) || p.Length > 1000))
            {
                Logger.LogInformation($"{nameof(AddFiles)}: One of the provided paths is not valid");
                throw new FileNotSupportedException("One of the provided paths is not valid", AppMessageType.FilesAreNotValid);
            }

            var playList = PlayLists.Find(pl => pl.Id == playListId);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(AddFiles)}: PlayListId = {playListId} does not exist");
                throw new PlayListNotFoundException($"PlayListId = {playListId} does not exist");
            }

            int startIndex = playList.Files.Count + 1;
            var files = paths.Where(path =>
            {
                var ext = Path.GetExtension(path);
                return FileFormatConstants.AllowedFormats.Contains(ext.ToLower()) && playList.Files.All(f => f.Path != path);
            }).OrderBy(p => p, new WindowsExplorerComparer())
                .Select((path, index) => new FileItem
                {
                    Position = startIndex + index,
                    PlayListId = playListId,
                    Path = path,
                    CreatedAt = DateTime.Now
                }).ToList();

            var createdFiles = await _appDataService.AddFiles(files);
            ServerService.OnFilesAdded.Invoke(playListId, createdFiles.ToArray());
        }

        public async Task AddFile(long playListId, FileItem file)
        {
            var playList = PlayLists.Find(f => f.Id == playListId);
            if (playList == null)
            {
                Logger.LogWarning($"{nameof(AddFile)}: PlayListId = {playListId} does not exist. It may have been removed");
                return;
            }

            var serverFileItem = ServerFileItem.From(FileService, file);
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
                Logger.LogInformation($"{nameof(AddUrl)}: {msg}");
                throw new InvalidRequestException(msg, AppMessageType.NoInternetConnection);
            }

            bool isUrlFile = FileService.IsUrlFile(url);
            if (!isUrlFile || !YoutubeUrlDecoder.IsYoutubeUrl(url))
            {
                var msg = $"Url = {url} is not supported";
                Logger.LogInformation($"{nameof(AddUrl)}: {msg}");
                throw new FileNotSupportedException(msg, AppMessageType.UrlNotSupported);
            }

            try
            {
                SendPlayListBusy(playListId, true);
                Logger.LogInformation($"{nameof(AddUrl)}: Trying to parse url = {url}");
                if (!YoutubeUrlDecoder.IsPlayList(url) || YoutubeUrlDecoder.IsPlayListAndVideo(url) && onlyVideo)
                {
                    Logger.LogInformation(
                        $"{nameof(AddUrl)}: Url is either not a playlist or we just want the video, parsing it...");
                    await AddYoutubeUrl(playListId, url);
                    return;
                }

                Logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist...");
                var links = await YoutubeUrlDecoder.ParseYouTubePlayList(url, FileCancellationTokenSource.Token);
                foreach (var link in links)
                {
                    if (FileCancellationTokenSource.IsCancellationRequested)
                        break;
                    Logger.LogInformation($"{nameof(AddUrl)}: Parsing playlist url = {link}");
                    await AddYoutubeUrl(playListId, link);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"{nameof(AddUrl)}: Couldn't parse url = {url}");
                TelemetryService.TrackError(e);
                throw;
            }
            finally
            {
                SendPlayListBusy(playListId, false);
            }
        }

        private async Task AddYoutubeUrl(long playListId, string url)
        {
            var media = await YoutubeUrlDecoder.Parse(url, null, false);
            if (!string.IsNullOrEmpty(media.Title) && media.Title.Length > AppWebServerConstants.MaxCharsPerString)
            {
                media.Title = media.Title.Substring(0, AppWebServerConstants.MaxCharsPerString);
            }
            var playList = GetPlayListInternal(playListId);
            var createdFile = await _appDataService.AddFile(playListId, url, playList.Files.Count + 1, media.Title);
            var serverFileItem = ServerFileItem.From(FileService, createdFile);
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
            Logger.LogWarning($"{nameof(GetPlayListInternal)}: PlaylistId = {id} doesn't exists");
            ServerService.OnServerMessage?.Invoke(AppMessageType.PlayListNotFound);
            if (throwOnNotFound)
                throw new PlayListNotFoundException($"PlayListId = {id} does not exist");
            return null;
        }
    }
}
