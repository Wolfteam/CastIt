using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface IServerCastService : ICastService
    {
        List<ServerPlayList> PlayLists { get; }
        ServerPlayList CurrentPlayList { get; }
        ServerFileItem CurrentPlayedFile { get; }

        Task PlayFile(long playListId, long id, bool force, bool fileOptionsChanged, bool isAnAutomaticCall);

        Task GoTo(bool nextTrack, bool isAnAutomaticCall = false);

        Task PlayFile(string fileName, bool force, bool isAnAutomaticCall);
        Task PlayFile(ServerFileItem file, bool force = false, bool isAnAutomaticCall = false);
        Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged, bool isAnAutomaticCall);
        FileItemResponseDto GetCurrentPlayedFile();
        Task UpdatePlayList(long playListId, string newName);
        void UpdatePlayListPosition(long playListId, int newIndex);
        void UpdateFilePosition(long playListId, long id, int newIndex);
        Task RemoveFilesThatStartsWith(long playListId, string path);
        Task RemoveAllMissingFiles(long playListId);
        Task RemoveFiles(long playListId, params long[] ids);
        void SortFiles(long playListId, SortModeType sortBy);
        void SetFilePositionIfChanged(long playListId);
        Task AddFolder(long playListId, bool includeSubFolders, string[] folders);
        Task AddFiles(long playListId, string[] paths);
        Task AddFile(long playListId, FileItem file);
        Task AddUrl(long playListId, string url, bool onlyVideo);
        Task AddFolderOrFileOrUrl(long playListId, string path, bool includeSubFolders, bool onlyVideo);
        ServerPlayerStatusResponseDto GetPlayerStatus();
        Task SetFileInfoForPendingFiles();

        Task UpdateFileItem(ServerFileItem file, bool force = true);
        void SetPlayListOptions(long id, bool loop, bool shuffle);
        void DisableLoopForAllFiles(long exceptFileId = -1);
        PlayListItemResponseDto GetPlayList(long playListId);
        PlayListItemResponseDto GetPlayList(string name);
        Task<List<GetAllPlayListResponseDto>> GetAllPlayLists();
        Task<PlayListItemResponseDto> AddNewPlayList();
        Task DeletePlayList(long playListId);
        Task DeleteAllPlayLists(long exceptId);
        void RefreshPlayListImages();

        Task StopPlayback(bool nullPlayedFile = true, bool disableLoopForAll = true);

        void CleanPlayedFile(bool nullPlayedFile = true);

        void LoopFile(long playListId, long id, bool loop);

        Task SetCurrentPlayedFileOptions(int audioStreamIndex, int subsStreamIndex, int qualityIndex);

        Task SetCurrentPlayedFileOptions(int streamIndex, bool isAudio, bool isSubTitle, bool isQuality);

        Task SetFileSubtitlesFromPath(string filePath);

        Task<byte[]> GetClosestPreviewThumbnail(long tentativeSecond);

        void ExchangeLastFilePosition(ServerPlayList playList, long toFileId);
    }
}
