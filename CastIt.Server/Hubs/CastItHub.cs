using CastIt.Application.Interfaces;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Infrastructure.Models;
using CastIt.Server.Common.Extensions;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Hubs
{
    //The exposed name methods here must match the ones that the client can call
    public class CastItHub : Hub<ICastItHub>
    {
        private readonly ILogger<CastItHub> _logger;
        private readonly IServerCastService _castService;
        private readonly IServerAppSettingsService _settingsService;
        private readonly ITelemetryService _telemetryService;

        public CastItHub(
            ILogger<CastItHub> logger,
            IServerCastService castService,
            IServerAppSettingsService settingsService,
            ITelemetryService telemetryService)
        {
            _logger = logger;
            _castService = castService;
            _settingsService = settingsService;
            _telemetryService = telemetryService;
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
        public async Task Play(PlayFileRequestDto dto)
        {
            try
            {
                await _castService.PlayFile(dto.PlayListId, dto.Id, dto.Force, dto.FileOptionsChanged);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task GoToSeconds(double seconds)
        {
            try
            {
                await _castService.GoToSeconds(seconds);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task GoToPosition(double position)
        {
            try
            {
                await _castService.GoToPosition(position);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task SkipSeconds(double seconds)
        {
            try
            {
                await _castService.AddSeconds(seconds);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task GoTo(bool next, bool previous)
        {
            try
            {
                if (next)
                    await _castService.GoTo(true);

                if (previous)
                    await _castService.GoTo(false);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task TogglePlayBack()
        {
            try
            {
                await _castService.TogglePlayback();
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task StopPlayback()
        {
            try
            {
                await _castService.StopPlayback();
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task DeleteFile(long playlistId, long id)
        {
            try
            {
                await _castService.RemoveFiles(playlistId, id);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public Task LoopFile(long playlistId, long id, bool loop)
        {
            try
            {
                _castService.LoopFile(playlistId, id, loop);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
            return Task.CompletedTask;
        }

        public async Task SetFileOptions(SetFileOptionsRequestDto dto)
        {
            try
            {
                await _castService.SetCurrentPlayedFileOptions(dto.StreamIndex, dto.IsAudio, dto.IsSubTitle, dto.IsQuality);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task UpdateSettings(ServerAppSettings settings)
        {
            try
            {
                await _settingsService.UpdateSettings(settings);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task SetVolume(SetVolumeRequestDto dto)
        {
            try
            {
                await _castService.SetVolume(dto.VolumeLevel);
                await _castService.SetIsMuted(dto.IsMuted);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
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

        public async Task UpdatePlayList(long id, UpdatePlayListRequestDto dto)
        {
            try
            {
                await _castService.UpdatePlayList(id, dto.Name);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
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

        public async Task DeletePlayList(long id)
        {
            try
            {
                await _castService.DeletePlayList(id);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task DeleteAllPlayLists(long exceptId = -1)
        {
            try
            {
                await _castService.DeleteAllPlayLists(exceptId);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task RemoveFiles(long id, List<long> ids)
        {
            try
            {
                await _castService.RemoveFiles(id, ids.ToArray());
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task RemoveFilesThatStartsWith(long playListId, string path)
        {
            try
            {
                await _castService.RemoveFilesThatStartsWith(playListId, path);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task RemoveAllMissingFiles(long playListId)
        {
            try
            {
                await _castService.RemoveAllMissingFiles(playListId);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task AddFolders(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            try
            {
                await _castService.AddFolder(playListId, dto.IncludeSubFolders, dto.Folders.ToArray());
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task AddFiles(long playListId, AddFolderOrFilesToPlayListRequestDto dto)
        {
            try
            {
                await _castService.AddFiles(playListId, dto.Files.ToArray());
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task AddUrlFile(long playListId, AddUrlToPlayListRequestDto dto)
        {
            try
            {
                await _castService.AddUrl(playListId, dto.Url, dto.OnlyVideo);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task AddFolderOrFileOrUrl(long playListId, AddFolderOrFileOrUrlToPlayListRequestDto dto)
        {
            try
            {
                await _castService.AddFolderOrFileOrUrl(playListId, dto.Path, dto.IncludeSubFolders, dto.OnlyVideo);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task SetFileSubtitlesFromPath(string filePath)
        {
            try
            {
                await _castService.SetFileSubtitlesFromPath(filePath);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task ConnectToCastDevice(string id)
        {
            try
            {
                await _castService.SetCastRenderer(id);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
        }

        public async Task RefreshCastDevices(TimeSpan? ts = null)
        {
            try
            {
                await _castService.RefreshCastDevices(ts);
            }
            catch (Exception e)
            {
                e.HandleCastException(_castService, _telemetryService);
            }
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
