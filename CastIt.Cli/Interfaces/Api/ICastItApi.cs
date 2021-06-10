using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Models.Device;
using CastIt.Infrastructure.Models;
using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CastIt.Cli.Interfaces.Api
{
    public interface ICastItApi
    {
        #region Player

        [Get("/Player/Devices")]
        Task<AppListResponseDto<Receiver>> GetAllDevices();

        [Post("/Player/Connect")]
        Task<EmptyResponseDto> Connect([Body] ConnectRequestDto dto);

        [Post("/Player/Disconnect")]
        Task<EmptyResponseDto> Disconnect();

        [Post("/Player/TogglePlayback")]
        Task<EmptyResponseDto> TogglePlayback();

        [Post("/Player/Stop")]
        Task<EmptyResponseDto> Stop();

        [Post("/Player/Volume")]
        Task<EmptyResponseDto> SetVolume([Body] SetVolumeRequestDto dto);

        [Post("/Player/Next")]
        Task<EmptyResponseDto> Next();

        [Post("/Player/Previous")]
        Task<EmptyResponseDto> Previous();

        [Post("/Player/GoToSeconds/{seconds}")]
        Task<EmptyResponseDto> GoToSeconds(double seconds);

        [Post("/Player/GoToPosition/{position}")]
        Task<EmptyResponseDto> GoToPosition(double position);

        [Post("/Player/Seek/{seconds}")]
        Task<EmptyResponseDto> Seek(double seconds);

        [Get("/Player/Settings")]
        Task<AppResponseDto<ServerAppSettings>> GetCurrentSettings();

        [Patch("/Player/Settings")]
        Task<EmptyResponseDto> UpdateSettings([Body] StringContent body);
        #endregion

        #region PlayLists
        [Get("/PlayLists")]
        Task<AppListResponseDto<GetAllPlayListResponseDto>> GetAllPlayLists();

        [Get("/PlayLists/{id}")]
        Task<AppResponseDto<PlayListItemResponseDto>> GetPlayList(long id);

        [Post("/PlayLists")]
        Task<AppResponseDto<PlayListItemResponseDto>> AddNewPlayList();

        [Put("/PlayLists/{id}")]
        Task<EmptyResponseDto> UpdatePlayList(long id, [Body] UpdatePlayListRequestDto dto);

        [Put("/PlayLists/{id}/SetOptions")]
        Task<EmptyResponseDto> SetOptions(long id, [Body] SetPlayListOptionsRequestDto dto);

        [Delete("/PlayLists/{id}/RemoveFilesThatStartsWith/{path}")]
        Task<EmptyResponseDto> RemoveFilesThatStartsWith(long id, string path);

        [Delete("/PlayLists/{id}/RemoveAllMissingFiles")]
        Task<EmptyResponseDto> RemoveAllMissingFiles(long id);

        [Delete("/PlayLists/{id}/RemoveFiles")]
        Task<EmptyResponseDto> RemoveFiles(long id, [Query(CollectionFormat.Multi)] List<long> fileIds);

        [Delete("/PlayLists/{id}")]
        Task<EmptyResponseDto> DeletePlayList(long id);

        [Delete("/PlayLists/All/{exceptId}")]
        Task<EmptyResponseDto> DeleteAllPlayList(long exceptId);

        [Put("/PlayLists/{id}/AddFolders")]
        Task<EmptyResponseDto> AddFolders(long id, [Body] AddFolderOrFilesToPlayListRequestDto dto);

        [Put("/PlayLists/{id}/AddFiles")]
        Task<EmptyResponseDto> AddFiles(long id, [Body] AddFolderOrFilesToPlayListRequestDto dto);

        [Put("/PlayLists/{id}/AddUrl")]
        Task<EmptyResponseDto> AddUrl(long id, [Body] AddUrlToPlayListRequestDto dto);
        #endregion

        #region Files
        [Post("/Files/{fileId}/Play")]
        Task<EmptyResponseDto> Play(long fileId);
        #endregion
    }
}
