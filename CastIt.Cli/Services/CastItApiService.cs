using CastIt.Cli.Common.Utils;
using CastIt.Cli.Interfaces.Api;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Models.Device;
using CastIt.Infrastructure.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CastIt.Cli.Services
{
    public class CastItApiService : BaseApiService, ICastItApiService
    {
        private ICastItApi _api;

        public ICastItApi Api
            => _api ??= RestService.For<ICastItApi>(ServerUtils.StartServerIfNotStarted());

        public CastItApiService(ILogger<CastItApiService> logger) : base(logger)
        {
        }

        #region Player
        public async Task<AppListResponseDto<Receiver>> GetAllDevices()
        {
            var response = new AppListResponseDto<Receiver>();
            try
            {
                response = await Api.GetAllDevices();
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

        public async Task<EmptyResponseDto> RefreshDevices(double seconds)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.RefreshDevices(seconds);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(RefreshDevices)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(RefreshDevices)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Connect(string host, int port)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.Connect(new ConnectRequestDto
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
                response = await Api.Disconnect();
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
                response = await Api.TogglePlayback();
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
                response = await Api.Stop();
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
                response = await Api.SetVolume(new SetVolumeRequestDto
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
                response = await Api.Next();
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
                response = await Api.Previous();
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
                response = await Api.GoToSeconds(seconds);
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
                response = await Api.GoToPosition(position);
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
                response = await Api.Seek(seconds);
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

        public async Task<AppResponseDto<ServerAppSettings>> GetCurrentSettings()
        {
            var response = new AppResponseDto<ServerAppSettings>();
            try
            {
                response = await Api.GetCurrentSettings();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GetCurrentSettings)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GetCurrentSettings)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> UpdateSettings(JsonPatchDocument<ServerAppSettings> patch)
        {
            var response = new EmptyResponseDto();
            try
            {
                var body = JsonConvert.SerializeObject(patch);
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                response = await Api.UpdateSettings(content);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(UpdateSettings)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(UpdateSettings)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<AppResponseDto<ServerPlayerStatusResponseDto>> GetStatus()
        {
            var response = new AppResponseDto<ServerPlayerStatusResponseDto>();
            try
            {
                response = await Api.GetStatus();
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(GetStatus)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(GetStatus)}: Unknown error occurred");
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
                response = await Api.GetAllPlayLists();
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

        public async Task<AppResponseDto<PlayListItemResponseDto>> GetPlayList(long id, string name)
        {
            var response = new AppResponseDto<PlayListItemResponseDto>();
            try
            {
                response = !string.IsNullOrWhiteSpace(name)
                    ? await Api.GetPlayList(name)
                    : await Api.GetPlayList(id);
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
                response = await Api.AddNewPlayList();
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

        public async Task<EmptyResponseDto> UpdatePlayList(long id, string name)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.UpdatePlayList(id, new UpdatePlayListRequestDto
                {
                    Name = name,
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

        public async Task<EmptyResponseDto> UpdatePlayListPosition(long id, int newIndex)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.UpdatePlayListPosition(id, newIndex);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(UpdatePlayListPosition)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(UpdatePlayListPosition)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> SetPlayListOptions(long id, bool loop, bool shuffle)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.SetPlayListOptions(id, new SetPlayListOptionsRequestDto
                {
                    Loop = loop,
                    Shuffle = shuffle
                });
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(SetPlayListOptions)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SetPlayListOptions)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> RemoveFilesThatStartsWith(long id, string path)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.RemoveFilesThatStartsWith(id, path);
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
                response = await Api.RemoveAllMissingFiles(id);
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
                response = await Api.RemoveFiles(id, fileIds);
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
                response = await Api.DeletePlayList(id);
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
                response = await Api.DeleteAllPlayList(exceptId);
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

        public async Task<EmptyResponseDto> AddFolders(long id, bool includeSubFolders, List<string> folders)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.AddFolders(id, new AddFolderOrFilesToPlayListRequestDto
                {
                    Folders = folders,
                    IncludeSubFolders = includeSubFolders,
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
                response = await Api.AddFiles(id, new AddFolderOrFilesToPlayListRequestDto
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
                response = await Api.AddUrl(id, new AddUrlToPlayListRequestDto
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

        public async Task<EmptyResponseDto> Play(long playListId, long fileId)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.Play(playListId, fileId);
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

        public async Task<EmptyResponseDto> UpdateFilePosition(long playListId, long fileId, int newIndex)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.UpdateFilePosition(playListId, fileId, newIndex);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(UpdateFilePosition)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(UpdateFilePosition)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> SortFiles(long id, SortModeType sortMode)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.SortFiles(id, sortMode);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(SortFiles)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SortFiles)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> LoopFile(long playListId, long fileId, bool loop)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.LoopFile(playListId, fileId, loop);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(LoopFile)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(LoopFile)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }

        public async Task<EmptyResponseDto> Play(string filename, bool force)
        {
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.Play(new PlayFileFromNameRequestDto
                {
                    Filename = filename,
                    Force = force
                });
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

        public async Task<EmptyResponseDto> SetCurrentPlayedFileOptions(int audioStreamIndex, int subsStreamIndex, int quality)
        {
            var request = new SetMultiFileOptionsRequestDto
            {
                AudioStreamIndex = audioStreamIndex,
                SubtitleStreamIndex = subsStreamIndex,
                Quality = quality
            };
            var response = new EmptyResponseDto();
            try
            {
                response = await Api.SetCurrentPlayedFileOptions(request);
            }
            catch (ApiException apiEx)
            {
                Logger.LogError(apiEx, $"{nameof(SetCurrentPlayedFileOptions)}: Api exception occurred");
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"{nameof(SetCurrentPlayedFileOptions)}: Unknown error occurred");
                HandleUnknownException(response);
            }
            return response;
        }
        #endregion
    }
}
