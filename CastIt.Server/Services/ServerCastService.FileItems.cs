using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Server.Common.Comparers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CastIt.Server.Services
{
    public partial class ServerCastService
    {
        public void LoopFile(long playListId, long id, bool loop)
        {
            var playList = GetPlayListInternal(playListId, false);
            var file = playList?.Files.Find(f => f.Id == id);
            if (file == null)
            {
                _logger.LogWarning($"{nameof(LoopFile)}: FileId = {id} associated to playListId = {playListId} does not exist");
                return;
            }

            bool triggerChange = file.Loop != loop;
            file.Loop = loop;

            DisableLoopForAllFiles(file.Id);

            if (triggerChange)
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
        }

        public void DisableLoopForAllFiles(long exceptFileId = -1)
        {
            var files = PlayLists.SelectMany(pl => pl.Files).Where(f => f.Loop && f.Id != exceptFileId).ToList();

            foreach (var file in files.Where(file => file.Loop))
            {
                file.Loop = false;
                SendFileChanged(_mapper.Map<FileItemResponseDto>(file));
            }
        }

        public void UpdateFilePosition(long playListId, long id, int newIndex)
        {
            var playList = GetPlayListInternal(playListId, false);
            if (playList == null)
            {
                _logger.LogWarning($"{nameof(UpdateFilePosition)}: PlaylistId = {playListId} doesn't exist");
                return;
            }

            var file = playList.Files.Find(f => f.Id == id);
            if (file == null || playList.Files.Count - 1 < newIndex)
            {
                _logger.LogWarning($"{nameof(UpdateFilePosition)}: FileId = {id} doesn't exist or the newIndex = {newIndex} is not valid");
                return;
            }

            var currentIndex = playList.Files.IndexOf(file);

            _logger.LogInformation($"{nameof(UpdateFilePosition)}: Moving file from index = {currentIndex} to newIndex = {newIndex}");
            playList.Files.RemoveAt(currentIndex);
            playList.Files.Insert(newIndex, file);
            SetFilePositionIfChanged(playListId);
        }

        public async Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            var separators = new[] { '/', '\\' };
            var playlist = GetPlayListInternal(playListId);
            var fileIds = playlist.Files
                .Where(f =>
                {
                    if (string.IsNullOrWhiteSpace(f.Path))
                        return false;
                    var filePath = string.Concat(f.Path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    var startsWith = string.Concat(path.Split(separators, StringSplitOptions.RemoveEmptyEntries));
                    return filePath.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase);
                })
                .Select(f => f.Id)
                .ToArray();
            if (!fileIds.Any())
                return;

            await RemoveFiles(playListId, fileIds);
        }

        public async Task RemoveAllMissingFiles(long playListId)
        {
            var playList = GetPlayListInternal(playListId);

            var fileIds = playList.Files.Where(f => !f.Exists).Select(f => f.Id).ToArray();
            if (!fileIds.Any())
                return;

            await RemoveFiles(playListId, fileIds);
        }

        public async Task RemoveFiles(long playListId, params long[] ids)
        {
            SendPlayListBusy(playListId, true);
            var playList = GetPlayListInternal(playListId);

            await _appDataService.DeleteFiles(ids.ToList());
            playList.Files.RemoveAll(f => ids.Contains(f.Id));
            foreach (var id in ids)
            {
                SendFileDeleted(playListId, id);
            }
            await AfterRemovingFiles(playListId);
            SendPlayListBusy(playListId, false);
        }

        private Task AfterRemovingFiles(long playListId)
        {
            var playList = GetPlayListInternal(playListId);
            SetFilePositionIfChanged(playListId);
            SendPlayListChanged(_mapper.Map<GetAllPlayListResponseDto>(playList));
            return Task.CompletedTask;
        }

        public void SortFiles(long playListId, SortModeType sortBy)
        {
            var playList = GetPlayListInternal(playListId, false);
            if (playList == null)
            {
                return;
            }

            if ((sortBy == SortModeType.DurationAsc || sortBy == SortModeType.DurationDesc) &&
                playList.Files.Any(f => string.IsNullOrEmpty(f.Duration)))
            {
                _server.OnServerMessage?.Invoke(AppMessageType.OneOrMoreFilesAreNotReadyYet);
                return;
            }

            playList.Files.Sort(new WindowsExplorerComparerForServerFileItem(sortBy));
            SetFilePositionIfChanged(playListId);
        }

        public void SetFilePositionIfChanged(long playListId)
        {
            var playList = GetPlayListInternal(playListId);
            for (int i = 0; i < playList.Files.Count; i++)
            {
                var item = playList.Files[i];
                var newIndex = i + 1;
                if (item.Position == newIndex)
                    continue;
                item.Position = newIndex;
                item.PositionChanged = true;
            }
            SendFilesChanged(_mapper.Map<List<FileItemResponseDto>>(playList.Files));
        }
    }
}
