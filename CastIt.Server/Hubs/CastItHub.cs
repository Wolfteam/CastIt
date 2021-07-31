using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Domain.Interfaces;
using CastIt.Infrastructure.Models;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Hubs
{
    //The exposed name methods here must match the ones that the client listens for
    public interface ICastItHub
    {
        Task SendPlayLists(List<GetAllPlayListResponseDto> playLists);

        Task StoppedPlayBack();

        Task PlayListAdded(GetAllPlayListResponseDto playList);

        Task PlayListChanged(GetAllPlayListResponseDto playList);

        Task PlayListsChanged(List<GetAllPlayListResponseDto> playLists);

        Task PlayListDeleted(long id);

        Task PlayListIsBusy(long id, bool isBusy);

        Task FileAdded(FileItemResponseDto file);

        Task FileChanged(FileItemResponseDto file);

        Task FilesChanged(List<FileItemResponseDto> files);

        Task FileDeleted(long playListId, long id);

        Task FileLoading(FileItemResponseDto file);

        Task FileLoaded(FileItemResponseDto file);

        Task FileEndReached(FileItemResponseDto file);

        //Task FileStoppedPlayback(FileItemResponseDto file);

        //Task CurrentPlayedFileStatusChanged(ServerFileItem file);

        Task PlayerStatusChanged(ServerPlayerStatusResponseDto status);

        Task PlayerSettingsChanged(ServerAppSettings settings);

        Task ServerMessage(AppMessageType type);

        Task CastDeviceSet(IReceiver device);

        Task CastDevicesChanged(List<IReceiver> devices);

        Task CastDeviceDisconnected();
    }

    //The exposed name methods here must match the ones that the client can call
    public class CastItHub : Hub<ICastItHub>
    {
        private readonly ILogger<CastItHub> _logger;
        private readonly IServerCastService _castService;
        private readonly IServerAppSettingsService _settingsService;

        public CastItHub(
            ILogger<CastItHub> logger,
            IServerCastService castService,
            IServerAppSettingsService settingsService)
        {
            _logger = logger;
            _castService = castService;
            _settingsService = settingsService;
        }

        public override async Task OnConnectedAsync()
        {
            await SendSettingsChanged();
            if (_castService.IsPlayingOrPaused)
            {
                await SendPlayerStatusChanged();
            }
            await SendPlayListsToClient();
            await SendCastDevicesChanged();
        }

        #region Client Msgs
        public Task Play(PlayFileRequestDto dto)
        {
            return _castService.PlayFile(dto.PlayListId, dto.Id, dto.Force, dto.FileOptionsChanged);
        }

        public Task GoToSeconds(double seconds)
        {
            return _castService.GoToSeconds(seconds);
        }

        public Task SkipSeconds(double seconds)
        {
            return _castService.AddSeconds(seconds);
        }

        public Task GoTo(bool next, bool previous)
        {
            if (next)
                return _castService.GoTo(true);

            if (previous)
                return _castService.GoTo(false);

            return Task.CompletedTask;
        }

        public Task TogglePlayBack()
        {
            return _castService.TogglePlayback();
        }

        public Task StopPlayback()
        {
            return _castService.StopPlayback();
        }

        public Task DeleteFile(long playlistId, long id)
        {
            return _castService.RemoveFiles(playlistId, id);
        }

        public Task LoopFile(long playlistId, long id, bool loop)
        {
            _castService.LoopFile(playlistId, id, loop);
            return Task.CompletedTask;
        }

        public Task SetFileOptions(SetFileOptionsRequestDto dto)
        {
            return _castService.SetCurrentPlayedFileOptions(dto.StreamIndex, dto.IsAudio, dto.IsSubTitle, dto.IsQuality);
        }

        public async Task UpdateSettings(ServerAppSettings settings)
        {
            await _settingsService.UpdateSettings(settings);
            await SendSettingsChanged();
        }

        public async Task SetVolume(SetVolumeRequestDto dto)
        {
            await _castService.SetVolume(dto.VolumeLevel);
            await _castService.SetIsMuted(dto.IsMuted);
        }

        public Task<PlayListItemResponseDto> AddNewPlayList()
        {
            return _castService.AddNewPlayList();
        }

        public Task<PlayListItemResponseDto> GetPlayList(long id)
        {
            var playList = _castService.GetPlayList(id);
            return Task.FromResult(playList);
        }

        public Task UpdatePlayList(long id, UpdatePlayListRequestDto dto)
        {
            return _castService.UpdatePlayList(id, dto.Name);
        }

        public Task UpdatePlayListPosition(long id, int newIndex)
        {
            _castService.UpdatePlayListPosition(id, newIndex);
            return Task.CompletedTask;
        }

        public Task SetPlayListOptions(long id, SetPlayListOptionsRequestDto dto)
        {
            _castService.SetPlayListOptions(id, dto.Loop, dto.Shuffle);
            return Task.CompletedTask;
        }

        public Task DeletePlayList(long id)
        {
            return _castService.DeletePlayList(id);
        }

        public Task DeleteAllPlayLists(long exceptId = -1)
        {
            return _castService.DeleteAllPlayLists(exceptId);
        }

        public Task RemoveFiles(long id, List<long> ids)
        {
            return _castService.RemoveFiles(id, ids.ToArray());
        }

        public Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            return _castService.RemoveFilesThatStartsWith(playListId, path);
        }

        public Task RemoveAllMissingFiles(long playListId)
        {
            return _castService.RemoveAllMissingFiles(playListId);
        }

        public Task AddFolders(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            return _castService.AddFolder(playListId, dto.IncludeSubFolders, dto.Folders.ToArray());
        }

        public Task AddFiles(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            return _castService.AddFiles(playListId, dto.Files.ToArray());
        }

        public Task AddUrlFile(long playListId, AddUrlToPlayListRequestDto dto)
        {
            return _castService.AddUrl(playListId, dto.Url, dto.OnlyVideo);
        }

        public Task SetFileSubtitlesFromPath(string filePath)
        {
            return _castService.SetFileSubtitlesFromPath(filePath);
        }

        public Task ConnectToCastDevice(string id)
        {
            return _castService.SetCastRenderer(id);
        }

        public Task RefreshCastDevices(TimeSpan ts)
        {
            return _castService.RefreshCastDevices(ts);
        }

        public Task SortFiles(long playListId, SortModeType sortMode)
        {
            _castService.SortFiles(playListId, sortMode);
            return Task.CompletedTask;
        }

        public Task UpdateFilePosition(long playListId, long id, int newIndex)
        {
            _castService.UpdateFilePosition(playListId, id, newIndex);
            return Task.CompletedTask;
        }
        #endregion

        #region Server Msgs
        public async Task SendPlayListsToClient()
        {
            var playLists = await _castService.GetAllPlayLists();
            await Clients.Caller.SendPlayLists(playLists);
        }

        public Task SendFileLoading(FileItemResponseDto file)
        {
            return Clients.All.FileLoading(file);
        }

        public Task SendFileLoaded(FileItemResponseDto file)
        {
            return Clients.All.FileLoaded(file);
        }

        public Task SendPlayerStatusChanged()
        {
            var status = _castService.GetPlayerStatus();
            return Clients.All.PlayerStatusChanged(status);
        }

        public Task SendEndReached(FileItemResponseDto file)
        {
            return Clients.All.FileEndReached(file);
        }

        public Task SendCastDevicesChanged()
        {
            var devices = _castService.AvailableDevices;
            return Clients.All.CastDevicesChanged(devices);
        }

        public Task SendSettingsChanged()
        {
            var settings = _settingsService.Settings;
            return Clients.All.PlayerSettingsChanged(settings);
        }

        public Task SendServerMessage(AppMessageType type)
        {
            return Clients.All.ServerMessage(type);
        }
        #endregion
    }
}
