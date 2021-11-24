using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Models.Device;
using CastIt.Infrastructure.Models;
using Microsoft.AspNetCore.JsonPatch;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Cli.Interfaces.Api
{
    public interface ICastItApiService
    {
        Task<AppListResponseDto<Receiver>> GetAllDevices();
        Task<EmptyResponseDto> RefreshDevices(double seconds);
        Task<EmptyResponseDto> Connect(string host, int port);
        Task<EmptyResponseDto> Disconnect();
        Task<EmptyResponseDto> TogglePlayback();
        Task<EmptyResponseDto> Stop();
        Task<EmptyResponseDto> SetVolume(double level, bool isMuted);
        Task<EmptyResponseDto> Next();
        Task<EmptyResponseDto> Previous();
        Task<EmptyResponseDto> GoToSeconds(double seconds);
        Task<EmptyResponseDto> GoToPosition(double position);
        Task<EmptyResponseDto> Seek(double seconds);
        Task<AppListResponseDto<GetAllPlayListResponseDto>> GetAllPlayLists();
        Task<AppResponseDto<PlayListItemResponseDto>> GetPlayList(long id, string name);
        Task<AppResponseDto<PlayListItemResponseDto>> AddNewPlayList();
        Task<EmptyResponseDto> UpdatePlayList(long id, string name);
        Task<EmptyResponseDto> UpdatePlayListPosition(long id, int newIndex);
        Task<EmptyResponseDto> SetPlayListOptions(long id, bool loop, bool shuffle);
        Task<EmptyResponseDto> RemoveFilesThatStartsWith(long id, string path);
        Task<EmptyResponseDto> RemoveAllMissingFiles(long id);
        Task<EmptyResponseDto> RemoveFiles(long id, List<long> fileIds);
        Task<EmptyResponseDto> DeletePlayList(long id);
        Task<EmptyResponseDto> DeleteAllPlayList(long exceptId);
        Task<EmptyResponseDto> AddFolders(long id, bool includeSubFolders, List<string> folders);
        Task<EmptyResponseDto> AddFiles(long id, List<string> files);
        Task<EmptyResponseDto> AddUrl(long id, string url, bool onlyVideo);
        Task<EmptyResponseDto> Play(long playListId, long fileId);
        Task<AppResponseDto<ServerAppSettings>> GetCurrentSettings();
        Task<EmptyResponseDto> UpdateSettings(JsonPatchDocument<ServerAppSettings> patch);
        Task<AppResponseDto<ServerPlayerStatusResponseDto>> GetStatus();
        Task<EmptyResponseDto> Play(string filename, bool force);
        Task<EmptyResponseDto> SetCurrentPlayedFileOptions(int audioStreamIndex, int subsStreamIndex, int quality);
        Task<EmptyResponseDto> LoopFile(long playListId, long fileId, bool loop);
        Task<EmptyResponseDto> UpdateFilePosition(long playListId, long fileId, int newIndex);
        Task<EmptyResponseDto> SortFiles(long id, [Query] SortModeType sortMode);
    }
}