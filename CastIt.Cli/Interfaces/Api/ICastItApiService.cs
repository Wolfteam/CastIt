using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Models.Device;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Cli.Interfaces.Api
{
    public interface ICastItApiService
    {
        Task<AppListResponseDto<Receiver>> GetAllDevices();
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
        Task<AppResponseDto<PlayListItemResponseDto>> GetPlayList(long id);
        Task<AppResponseDto<PlayListItemResponseDto>> AddNewPlayList();
        Task<EmptyResponseDto> UpdatePlayList(long id, string name, int? position);
        Task<EmptyResponseDto> SetOptions(long id, bool loop, bool shuffle);
        Task<EmptyResponseDto> RemoveFilesThatStartsWith(long id, string path);
        Task<EmptyResponseDto> RemoveAllMissingFiles(long id);
        Task<EmptyResponseDto> RemoveFiles(long id, List<long> fileIds);
        Task<EmptyResponseDto> DeletePlayList(long id);
        Task<EmptyResponseDto> DeleteAllPlayList(long exceptId);
        Task<EmptyResponseDto> AddFolders(long id, List<string> folders);
        Task<EmptyResponseDto> AddFiles(long id, List<string> files);
        Task<EmptyResponseDto> AddUrl(long id, string url, bool onlyVideo);
        Task<EmptyResponseDto> Play(long fileId);
    }
}