using AutoMapper;
using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CastIt.Server.Interfaces;

namespace CastIt.Server.Hubs
{
    //The exposed name methods here must match the ones that the client listens for
    public interface ICastItHub
    {
        //Task OnConnectedAsync();
        Task Play(PlayFileRequestDto dto);
        //Task GoToSeconds(GoToSecondsRequestDto dto);
        //Task SkipSeconds(GoToSecondsRequestDto dto);
        //Task GoTo(GoToRequestDto dto);
        //Task TogglePlayBack();
        //Task StopPlayBack();
        //Task SetPlayListOptions(SetPlayListOptionsRequestDto dto);
        //Task DeletePlayList(long id);
        //Task DeleteFile(long id, long playlistId);
        //Task LoopFile(SetLoopFileRequestDto dto);
        //Task SetFileOptions(SetFileOptionsRequestDto dto);
        //Task UpdateSettings(AppSettingsRequestDto dto);
        //Task SetVolume(SetVolumeRequestDto dto);
        //Task RenamePlayList(RenamePlayListRequestDto dto);
        //Task CloseApp();
        //Task SendPlayLists();
        //Task GetPlayList(long playlistId);
        //Task ClientConnected();
        //void FileLoading();
        //Task SendCurrentFileLoaded();
        //void FileLoadingError(string error);
        //void EndReached();
        Task PositionChanged(double position);
        //void TimeChanged(double seconds);
        //void Paused();
        //void ChromeCastDisconnected();
        //void VolumeLevelChanged(double newLevel, bool isMuted);
        //void AppClosing();
        //Task RefreshPlayList(long id, bool wasDeleted = false);
        //void SendInfoMessage(string msg);
        //Task SendMsg(string msgType, bool succeed = true);
        //Task SendMsg<T>(T result, string msgType, bool succeed = true) where T : class;

        Task SendMsg(AppMessageType type);
        Task EndReached();
        Task ShutDown();
        Task SendMsg(EmptyResponseDto dto);

        Task SendPlayLists(List<GetAllPlayListResponseDto> playLists);
    }

    //The exposed name methods here must match the ones that the client can call
    public class CastItHub : Hub<ICastItHub>
    {
        #region Server Constants
        private const string PlayListsLoadedMsgType = "SERVER_PLAYLISTS_ALL";
        private const string PlayListLoadedMsgType = "SERVER_PLAYLISTS_ONE";
        private const string RefreshPlayListMsgType = "SERVER_PLAYLIST_REFRESH";

        private const string FileLoadingMsgType = "SERVER_FILE_LOADING";
        private const string FileLoadedMsgType = "SERVER_FILE_LOADED";
        private const string FilePositionChangedMsgType = "SERVER_FILE_POSITION_CHANGED";
        private const string FileTimeChangedMsgType = "SERVER_FILE_TIME_CHANGED";
        private const string FilePausedMsgType = "SERVER_FILE_PAUSED";
        private const string FileEndReachedMsgType = "SERVER_FILE_END_REACHED";
        private const string ErrorOnFileLoadingMsgType = "SERVER_ERROR_ON_FILE_LOADING";
        private const string SendFileOptionsMsgType = "SERVER_SEND_FILE_OPTIONS";

        private const string ClientConnectedMsgType = "SERVER_CLIENT_CONNECTED";
        private const string SettingsChangedMsgType = "SERVER_SETTINGS_CHANGED";
        private const string ChromeCastDisconnectedMsgType = "SERVER_CHROMECAST_DISCONNECTED";
        private const string VolumeChangedMsgType = "SERVER_VOLUME_LEVEL_CHANGED";
        private const string AppClosedMsgType = "SERVER_APP_CLOSING";
        private const string InfoMsgType = "SERVER_INFO_MSG";
        #endregion

        private readonly ILogger<CastItHub> _logger;
        private readonly IServerCastService _castService;
        private readonly IMapper _mapper;

        public CastItHub(ILogger<CastItHub> logger, IServerCastService castService, IMapper mapper)
        {
            _logger = logger;
            _castService = castService;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var url = GetBaseUrl();
            await SendClientConnected().ConfigureAwait(false);
            if (_castService.IsPlayingOrPaused)
            {
                await SendCurrentFileLoadedToClient();
            }
            //await AppSettingsChangedTask().ConfigureAwait(false);

            await SendPlayListsToClient();
        }

        #region Client Msgs

        public Task Play(PlayFileRequestDto dto)
        {
            return _castService.PlayFile(dto.PlayListId, dto.Force, false);
        }

        public Task GoToSeconds(GoToSecondsRequestDto dto)
        {
            return _castService.GoToSeconds(dto.Seconds);
        }

        public Task SkipSeconds(GoToSecondsRequestDto dto)
        {
            return _castService.AddSeconds(dto.Seconds);
        }

        public Task GoTo(GoToRequestDto dto)
        {
            if (dto.Next)
                return _castService.GoTo(true);

            if (dto.Previous)
                return _castService.GoTo(false);

            return Task.CompletedTask;
        }

        public Task TogglePlayBack()
        {
            return _castService.TogglePlayback();
        }

        public Task StopPlayBack()
        {
            return _castService.StopPlayback();
        }

        public Task SetPlayListOptions(SetPlayListOptionsRequestDto dto)
        {
            return Task.CompletedTask;
            //_view.SetPlayListOptions(playlistOptions.Id, playlistOptions.Loop, playlistOptions.Shuffle);
        }

        public Task DeletePlayList(long id)
        {
            return Task.CompletedTask;
            //return _view.DeletePlayList(deletePlayListRequest.Id);
        }

        public Task DeleteFile(long id, long playlistId)
        {
            return Task.CompletedTask;
            //return _view.DeleteFile(deleteFileRequest.Id, deleteFileRequest.PlayListId);
        }

        public Task LoopFile(SetLoopFileRequestDto dto)
        {
            return Task.CompletedTask;
            //return _view.SetFileLoop(loopRequest.Id, loopRequest.PlayListId, loopRequest.Loop);
        }

        public Task SetFileOptions(SetFileOptionsRequestDto dto)
        {
            return Task.CompletedTask;
            //return _view.SetFileOptions(request.StreamIndex, request.IsAudio, request.IsSubTitle, request.IsQuality);
        }

        public Task UpdateSettings(AppSettingsRequestDto dto)
        {
            return Task.CompletedTask;
            //_view.UpdateSettings(
            //    settingsRequest.StartFilesFromTheStart,
            //    settingsRequest.PlayNextFileAutomatically,
            //    settingsRequest.ForceVideoTranscode,
            //    settingsRequest.ForceAudioTranscode,
            //    settingsRequest.VideoScale,
            //    settingsRequest.EnableHardwareAcceleration);
        }

        public async Task SetVolume(SetVolumeRequestDto dto)
        {
            await _castService.SetVolume(dto.VolumeLevel);
            await _castService.SetIsMuted(dto.IsMuted);
        }

        public Task RenamePlayList(UpdatePlayListRequestDto dto)
        {
            return Task.CompletedTask;
            //return _view.RenamePlayList(renameRequest.Id, renameRequest.Name);
        }

        public Task CloseApp()
        {
            return Task.CompletedTask;
        }
        #endregion

        #region Server Msgs
        private async Task SendPlayListsToClient()
        {
            var playLists = _castService.PlayLists;
            var mapped = _mapper.Map<List<GetAllPlayListResponseDto>>(playLists);
            await Clients.Caller.SendPlayLists(mapped);
        }

        public async Task SendPlayListToClient(long playlistId)
        {
            var playlist = _castService.PlayLists.FirstOrDefault(pl => pl.Id == playlistId);
            await SendMsg(playlist, PlayListLoadedMsgType);
        }

        public async Task SendClientConnected()
        {
            await SendMsg(ClientConnectedMsgType).ConfigureAwait(false);
        }

        public Task<long> Testing(long id)
        {
            return Task.FromResult(id);
        }

        public async void SendFileLoadingToClient()
        {
            await SendMsg(FileLoadingMsgType).ConfigureAwait(false);
        }

        //public async void FileLoaded()
        //{
        //    await FileLoadedTask().ConfigureAwait(false);
        //}

        public Task SendCurrentFileLoadedToClient()
        {
            var file = _castService.GetCurrentFileLoaded();
            return file == null ? Task.CompletedTask : SendMsg(file, FileLoadedMsgType);
        }

        public async void FileLoadingError(string error)
        {
            await SendMsg(ErrorOnFileLoadingMsgType).ConfigureAwait(false);
        }

        public async void EndReached()
        {
            await SendMsg(FileEndReachedMsgType).ConfigureAwait(false);
        }

        public async void SendPositionChangedToClients(double position)
        {
            var response = new SocketResponseDto<double>
            {
                MessageType = FilePositionChangedMsgType,
                Succeed = true,
                Result = position,
            };
            await SendMsgToClients(response).ConfigureAwait(false);
        }

        public async void TimeChanged(double seconds)
        {
            var response = new SocketResponseDto<double>
            {
                MessageType = FileTimeChangedMsgType,
                Succeed = true,
                Result = seconds,
            };
            await SendMsgToClients(response).ConfigureAwait(false);
        }

        public async void Paused()
        {
            await SendMsg(FilePausedMsgType).ConfigureAwait(false);
        }

        public async void ChromeCastDisconnected()
        {
            await SendMsg(ChromeCastDisconnectedMsgType).ConfigureAwait(false);
        }

        public async void VolumeLevelChanged(double newLevel, bool isMuted)
        {
            var result = new VolumeLevelChangedResponseDto
            {
                IsMuted = isMuted,
                VolumeLevel = newLevel,
            };
            await SendMsg(result, VolumeChangedMsgType).ConfigureAwait(false);
        }

        public async void AppClosing()
        {
            await SendMsg(AppClosedMsgType).ConfigureAwait(false);
        }

        //public async void AppSettingsChanged()
        //{
        //    await AppSettingsChangedTask().ConfigureAwait(false);
        //}

        //public Task AppSettingsChangedTask()
        //{
        //    var settings = _view.GetCurrentAppSettings();
        //    return SendMsg(settings, SettingsChangedMsgType);
        //}

        //public async void PlayListChanged(long id)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(id).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async void PlayListDeleted(long id)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await RefreshPlayList(id, true).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        public Task RefreshPlayList(long id, bool wasDeleted = false)
        {
            var dto = new RefreshPlayListResponseDto
            {
                Id = id,
                WasDeleted = wasDeleted
            };

            return SendMsg(dto, RefreshPlayListMsgType);
        }

        //public async void FileAdded(long onPlayListId)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //}

        //public async void FileChanged(long onPlayListId)
        //{
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        //public async void FilesDeleted(long onPlayListId)
        //{
        //    await SendPlayLists().ConfigureAwait(false);
        //    await SendPlayList(onPlayListId).ConfigureAwait(false);
        //    if (_view.IsCurrentlyPlaying)
        //    {
        //        await FileLoadedTask().ConfigureAwait(false);
        //    }
        //}

        public async void SendInfoMessage(string msg)
        {
            await SendMsg(msg, InfoMsgType).ConfigureAwait(false);
        }

        //public async Task SendFileOptions(long id)
        //{
        //    var options = _view.GetFileOptions(id);
        //    await SendMsg(options, SendFileOptionsMsgType).ConfigureAwait(false);
        //    VolumeLevelChanged(_view.VolumeLevel, _view.IsMuted);
        //}

        private Task SendMsg(string msgType, bool succeed = true)
        {
            var response = new EmptySocketResponseDto
            {
                MessageType = msgType,
                Succeed = succeed,
            };
            if (msgType != FilePausedMsgType)
                _logger.LogInformation($"{nameof(SendMsgToClients)}: Sending msg of type = {msgType}");
            return SendMsgToClients(response);
        }

        private Task SendMsg<T>(T result, string msgType, bool succeed = true) where T : class
        {
            var response = new SocketResponseDto<T>
            {
                Succeed = succeed,
                MessageType = msgType,
                Result = result
            };
            _logger.LogInformation($"{nameof(SendMsgToClients)}: Sending msg of type = {msgType} with result");
            return SendMsgToClients(response);
        }

        private Task SendMsgToClients(EmptyResponseDto response)
        {
            //string json = JsonConvert.SerializeObject(response);
            return Clients.All.SendMsg(response);
        }

        private string GetBaseUrl()
        {
            var feature = Context.Features.Get<IHttpConnectionFeature>();
            var url = $"{feature.LocalIpAddress}/{feature.LocalPort}";
            return url;
        }
        #endregion
    }
}
