using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Models.Device;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Cli.Services
{
    public class CastItApiService : BaseApiService, ICastItApiService
    {
        private readonly ICastItApi _api;
        public CastItApiService(ILogger<CastItApiService> logger, ICastItApi api) : base(logger)
        {
            _api = api;
        }

        #region Player
        public async Task<AppListResponseDto<Receiver>> GetAllDevices()
        {
            var response = new AppListResponseDto<Receiver>();
            try
            {
                response = await _api.GetAllDevices();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GetAllDevices)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GetAllDevices)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Connect(string host, int port)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Connect(new ConnectRequestDto
                {
                    Port = port,
                    Host = host
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Connect)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Connect)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Disconnect()
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Disconnect();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Disconnect)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Disconnect)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> TogglePlayback()
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.TogglePlayback();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(TogglePlayback)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(TogglePlayback)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Stop()
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Stop();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Stop)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Stop)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> SetVolume(double level, bool isMuted)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.SetVolume(new SetVolumeRequestDto
                {
                    VolumeLevel = level,
                    IsMuted = isMuted
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(SetVolume)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SetVolume)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Next()
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Next();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Next)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Next)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Previous()
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Previous();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Previous)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Previous)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> GoToSeconds(double seconds)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.GoToSeconds(seconds);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GoToSeconds)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GoToSeconds)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> GoToPosition(double position)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.GoToPosition(position);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GoToPosition)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GoToPosition)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Seek(double seconds)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Seek(seconds);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Seek)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Seek)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        #endregion

        #region PlayLists
        public async Task<AppListResponseDto<GetAllPlayListResponseDto>> GetAllPlayLists()
        {
            var response = new AppListResponseDto<GetAllPlayListResponseDto>();
            try
            {
                response = await _api.GetAllPlayLists();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GetAllPlayLists)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GetAllPlayLists)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<AppResponseDto<PlayListItemResponseDto>> GetPlayList(long id)
        {
            var response = new AppResponseDto<PlayListItemResponseDto>();
            try
            {
                response = await _api.GetPlayList(id);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GetPlayList)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GetPlayList)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<AppResponseDto<PlayListItemResponseDto>> AddNewPlayList()
        {
            var response = new AppResponseDto<PlayListItemResponseDto>();
            try
            {
                response = await _api.AddNewPlayList();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(AddNewPlayList)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(AddNewPlayList)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> UpdatePlayList(long id, string name, int? position)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.UpdatePlayList(id, new UpdatePlayListRequestDto
                {
                    Name = name,
                    Position = position
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(UpdatePlayList)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(UpdatePlayList)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> SetOptions(long id, bool loop, bool shuffle)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.SetOptions(id, new SetPlayListOptionsRequestDto
                {
                    Loop = loop,
                    Shuffle = shuffle
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(SetOptions)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SetOptions)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> RemoveFilesThatStartsWith(long id, string path)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.RemoveFilesThatStartsWith(id, path);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(RemoveFilesThatStartsWith)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(RemoveFilesThatStartsWith)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> RemoveAllMissingFiles(long id)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.RemoveAllMissingFiles(id);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(RemoveAllMissingFiles)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(RemoveAllMissingFiles)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> RemoveFiles(long id, List<long> fileIds)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.RemoveFiles(id, fileIds);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(RemoveFiles)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(RemoveFiles)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> DeletePlayList(long id)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.DeletePlayList(id);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(DeletePlayList)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(DeletePlayList)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> DeleteAllPlayList(long exceptId)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.DeleteAllPlayList(exceptId);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(DeleteAllPlayList)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(DeleteAllPlayList)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> AddFolders(long id, List<string> folders)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.AddFolders(id, new AddFolderOrFilesToPlayListRequestDto
                {
                    Folders = folders
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(AddFolders)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(AddFolders)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> AddFiles(long id, List<string> files)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.AddFolders(id, new AddFolderOrFilesToPlayListRequestDto
                {
                    Files = files
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(AddFiles)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(AddFiles)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> AddUrl(long id, string url, bool onlyVideo)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.AddUrl(id, new AddUrlToPlayListRequestDto
                {
                    OnlyVideo = onlyVideo,
                    Url = url
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(AddUrl)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(AddUrl)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }
        #endregion

        #region Files
        public async Task<EmptyResponseDto> Play(long fileId)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await _api.Play(fileId);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(Play)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(Play)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }
        #endregion
    }
}
