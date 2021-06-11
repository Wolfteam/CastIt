using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Entities;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Models;
using CastIt.Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Interfaces
{
    public interface IServerCastService : ICastService
    {
        ServerFileItem CurrentPlayedFile { get; }

        List<ServerPlayList> PlayLists { get; }

        OnFilesAdded OnFilesAdded { get; set; }

        public Task PlayFile(long id, bool force, bool fileOptionsChanged);

        public Task GoTo(bool nextTrack, bool isAnAutomaticCall = false);

        //NPI SI ESTOS SE QUEDARAN ACA
        Task PlayFile(ServerFileItem file, bool force = false);
        Task PlayFile(ServerFileItem file, bool force, bool fileOptionsChanged);
        Task UpdatePlayList(long playListId, string newName, int? position = -1);
        Task RemoveFilesThatStartsWith(long playListId, string path);
        Task RemoveAllMissingFiles(long playListId);
        Task RemoveFiles(long playListId, params long[] ids);
        void SortFiles(long playListId, SortModeType sortBy);
        void SetPositionIfChanged(long playListId);
        Task AddFolder(long playListId, string[] folders);
        Task AddFiles(long playListId, string[] paths);
        Task AddFile(long playListId, FileItem file);
        Task AddUrl(long playListId, string url, bool onlyVideo);
        ServerPlayerStatus GetPlayerStatus();
        FileLoadedResponseDto GetCurrentFileLoaded();
        void SetPlayListOptions(long id, bool loop, bool shuffle);
        void DisableLoopForAllFiles(long exceptFileId = -1);
        PlayListItemResponseDto GetPlayList(long playListId);
        Task<List<GetAllPlayListResponseDto>> GetAllPlayLists();
        Task<PlayListItemResponseDto> AddNewPlayList();
        Task DeletePlayList(long playListId);
        Task DeleteAllPlayLists(long exceptId);
    }
}
