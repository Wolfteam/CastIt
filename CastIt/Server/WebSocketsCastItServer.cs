﻿using CastIt.Interfaces;
using CastIt.Interfaces.ViewModels;
using CastIt.Server.Dtos;
using CastIt.Server.Dtos.Requests;
using CastIt.Server.Dtos.Responses;
using EmbedIO.WebSockets;
using MvvmCross.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server
{
    public class WebSocketsCastItServer : WebSocketModule
    {
        #region Client Constants
        private const string GetPlayListsMsgType = "CLIENT_PLAYLISTS_ALL";
        private const string GetPlayListMsgType = "CLIENT_PLAYLIST_ONE";

        private const string PlayMsgType = "CLIENT_PLAYBLACK_PLAY";
        private const string GoToSecondsMsgType = "CLIENT_PLAYBLACK_GOTO_SECONDS";
        private const string SkipSecondsMsgType = "CLIENT_PLAYBLACK_SKIP_SECONDS";
        private const string GotoMsgType = "CLIENT_PLAYBLACK_GOTO";
        private const string TogglePlayBackMsgType = "CLIENT_PLAYBLACK_TOGGLE";
        private const string StopMsgType = "CLIENT_PLAYBACK_STOP";

        private const string SetPlayListOptionsMsgType = "CLIENT_PLAYLIST_OPTIONS";
        private const string DeletePlayListMsgType = "CLIENT_PLAYLIST_DELETE";
        private const string RenamePlayListMsgType = "CLIENT_PLAYLIST_RENAME";

        private const string DeleteFileMsgType = "CLIENT_FILE_DELETE";
        private const string LoopFileMsgType = "CLIENT_FILE_LOOP";
        private const string SetFileOptionsMsgType = "CLIENT_FILE_SET_OPTIONS";
        private const string GetFileOptionsMsgType = "CLIENT_GET_FILE_OPTIONS";

        private const string UpdateSettingsMsgType = "CLIENT_SETTINGS_UPDATE";
        private const string SetVolumeMsgType = "CLIENT_SET_VOLUME";
        private const string CloseAppMsgType = "CLIENT_CLOSE_APP";
        #endregion

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

        private readonly IMvxLog _logger;
        private readonly IAppSettingsService _appSettings;
        private readonly IMainViewModel _mainViewModel;

        public WebSocketsCastItServer(
            IMvxLog logger,
            IAppWebServer server,
            IAppSettingsService appSettings,
            IMainViewModel mainViewModel,
            string urlPath)
            : base(urlPath, true)
        {
            _logger = logger;
            _appSettings = appSettings;
            _mainViewModel = mainViewModel;
            server.OnFileLoading += FileLoading;
            server.OnFileLoaded += FileLoaded;
            server.OnFileLoadingError += FileLoadingError;
            server.OnEndReached += EndReached;
            server.OnTimeChanged += TimeChanged;
            server.OnPositionChanged += PositionChanged;
            server.OnPaused += Paused;
            server.OnDisconnected += ChromecastDisconnected;
            server.OnVolumeChanged += VolumeLevelChanged;
            server.OnAppClosing += AppClosing;
            server.OnAppSettingsChanged += AppSettingsChanged;
            server.OnPlayListAdded += PlayListAdded;
            server.OnPlayListChanged += PlayListChanged;
            server.OnPlayListDeleted += PlayListDeleted;
            server.OnFileAdded += FileAdded;
            server.OnFileChanged += FileChanged;
            server.OnFileDeleted += FilesDeleted;
            server.OnServerMsg += SendInfoMessage;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            return HandleMessage(context, buffer);
        }

        protected override async Task OnClientConnectedAsync(IWebSocketContext context)
        {
            _logger.Info($"{nameof(OnClientConnectedAsync)}: Client connected = {context.Id} - IP = {context.RemoteEndPoint}");
            await ClientConnected().ConfigureAwait(false);
            if (_mainViewModel.IsCurrentlyPlaying)
            {
                await FileLoadedTask().ConfigureAwait(false);
            }
            await AppSettingsChangedTask().ConfigureAwait(false);

            await SendPlayLists().ConfigureAwait(false);
        }

        #region Client Msgs
        private Task HandleMessage(IWebSocketContext context, byte[] buffer)
        {
            try
            {
                string msg = Encoding.GetString(buffer);
                var baseMsg = JsonConvert.DeserializeObject<BaseSocketRequestDto>(msg);
                _logger.Info(
                    $"{nameof(HandleMessage)}: Received msgType = {baseMsg.MessageType} " +
                    $"from clientId = {context.Id} - IP = {context.RemoteEndPoint}");
                switch (baseMsg.MessageType)
                {
                    case GetPlayListsMsgType:
                        return SendPlayLists();
                    case GetPlayListMsgType:
                        var getPlayListRequest = JsonConvert.DeserializeObject<BaseItemRequestDto>(msg);
                        return SendPlayList(getPlayListRequest.Id);
                    case PlayMsgType:
                        var playRequest = JsonConvert.DeserializeObject<PlayFileRequestDto>(msg);
                        return _mainViewModel.PlayFile(playRequest.Id, playRequest.PlayListId, playRequest.Force);
                    case GoToSecondsMsgType:
                        var gotoSecondsRequest = JsonConvert.DeserializeObject<GoToSecondsRequestDto>(msg);
                        return _mainViewModel.GoToSecondsCommand.ExecuteAsync(Convert.ToInt64(gotoSecondsRequest.Seconds));
                    case SkipSecondsMsgType:
                        var skipSecondsRequest = JsonConvert.DeserializeObject<GoToSecondsRequestDto>(msg);
                        return _mainViewModel.SkipCommand.ExecuteAsync(Convert.ToInt32(skipSecondsRequest.Seconds));
                    case GotoMsgType:
                        var gotoRequest = JsonConvert.DeserializeObject<GoToRequestDto>(msg);
                        if (gotoRequest.Next)
                            _mainViewModel.NextCommand.Execute();
                        else if (gotoRequest.Previous)
                            _mainViewModel.PreviousCommand.Execute();
                        break;
                    case TogglePlayBackMsgType:
                        _mainViewModel.TogglePlayBackCommand.Execute();
                        break;
                    case StopMsgType:
                        return _mainViewModel.StopPlayBackCommand.ExecuteAsync();
                    case SetPlayListOptionsMsgType:
                        var playlistOptions = JsonConvert.DeserializeObject<SetPlayListOptionsRequestDto>(msg);
                        _mainViewModel.SetPlayListOptions(playlistOptions.Id, playlistOptions.Loop, playlistOptions.Shuffle);
                        break;
                    case DeletePlayListMsgType:
                        var deletePlayListRequest = JsonConvert.DeserializeObject<BaseItemRequestDto>(msg);
                        return _mainViewModel.DeletePlayList(deletePlayListRequest.Id);
                    case DeleteFileMsgType:
                        var deleteFileRequest = JsonConvert.DeserializeObject<DeleteFileRequestDto>(msg);
                        return _mainViewModel.DeleteFile(deleteFileRequest.Id, deleteFileRequest.PlayListId);
                    case LoopFileMsgType:
                        var loopRequest = JsonConvert.DeserializeObject<SetLoopFileRequestDto>(msg);
                        return _mainViewModel.SetFileLoop(loopRequest.Id, loopRequest.PlayListId, loopRequest.Loop);
                    case SetFileOptionsMsgType:
                        var request = JsonConvert.DeserializeObject<SetFileOptionsRequestDto>(msg);
                        return _mainViewModel.SetFileOptions(request.StreamIndex, request.IsAudio, request.IsSubTitle, request.IsQuality);
                    case UpdateSettingsMsgType:
                        var settingsRequest = JsonConvert.DeserializeObject<AppSettingsRequestDto>(msg);
                        _mainViewModel.UpdateSettings(
                            settingsRequest.StartFilesFromTheStart,
                            settingsRequest.PlayNextFileAutomatically,
                            settingsRequest.ForceVideoTranscode,
                            settingsRequest.ForceAudioTranscode,
                            settingsRequest.VideoScale,
                            settingsRequest.EnableHardwareAcceleration);
                        break;
                    case GetFileOptionsMsgType:
                        var getFileOptionsRequest = JsonConvert.DeserializeObject<BaseItemRequestDto>(msg);
                        return SendFileOptions(getFileOptionsRequest.Id);
                    case SetVolumeMsgType:
                        var setVolumeRequest = JsonConvert.DeserializeObject<SetVolumeRequestDto>(msg);
                        _mainViewModel.VolumeLevel = setVolumeRequest.VolumeLevel;
                        var tasks = new List<Task>
                        {
                            _mainViewModel.SetVolumeCommand.ExecuteAsync()
                        };
                        if (_mainViewModel.IsMuted != setVolumeRequest.IsMuted)
                        {
                            tasks.Add(_mainViewModel.ToggleMuteCommand.ExecuteAsync());
                        }
                        return Task.WhenAll(tasks);
                    case RenamePlayListMsgType:
                        var renameRequest = JsonConvert.DeserializeObject<RenamePlayListRequestDto>(msg);
                        return _mainViewModel.RenamePlayList(renameRequest.Id, renameRequest.Name);
                    case CloseAppMsgType:
                        return _mainViewModel.CloseAppCommand.ExecuteAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unknown error occurred handling msg");
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Server Msgs
        private Task SendPlayLists()
        {
            var playlists = _mainViewModel.GetAllPlayLists();
            return SendMsg(playlists, PlayListsLoadedMsgType);
        }

        private Task SendPlayList(long playlistId)
        {
            var playlist = _mainViewModel.GetPlayList(playlistId);
            return SendMsg(playlist, PlayListLoadedMsgType);
        }

        private async Task ClientConnected()
        {
            await SendMsg(ClientConnectedMsgType).ConfigureAwait(false);
        }

        private async void FileLoading()
        {
            await SendMsg(FileLoadingMsgType).ConfigureAwait(false);
        }

        private async void FileLoaded()
        {
            await FileLoadedTask().ConfigureAwait(false);
        }

        private Task FileLoadedTask()
        {
            var file = _mainViewModel.GetCurrentFileLoaded();
            return file == null ? Task.CompletedTask : SendMsg(file, FileLoadedMsgType);
        }

        private async void FileLoadingError(string error)
        {
            await SendMsg(ErrorOnFileLoadingMsgType).ConfigureAwait(false);
        }

        private async void EndReached()
        {
            await SendMsg(FileEndReachedMsgType).ConfigureAwait(false);
        }

        private async void PositionChanged(double position)
        {
            var response = new SocketResponseDto<double>
            {
                MessageType = FilePositionChangedMsgType,
                Succeed = true,
                Result = position,
            };
            await SendMsg(response).ConfigureAwait(false);
        }

        private async void TimeChanged(double seconds)
        {
            var response = new SocketResponseDto<double>
            {
                MessageType = FileTimeChangedMsgType,
                Succeed = true,
                Result = seconds,
            };
            await SendMsg(response).ConfigureAwait(false);
        }

        private async void Paused()
        {
            await SendMsg(FilePausedMsgType).ConfigureAwait(false);
        }

        private async void ChromecastDisconnected()
        {
            await SendMsg(ChromeCastDisconnectedMsgType).ConfigureAwait(false);
        }

        private async void VolumeLevelChanged(double newLevel, bool isMuted)
        {
            var result = new VolumeLevelChangedResponseDto
            {
                IsMuted = isMuted,
                VolumeLevel = newLevel,
            };
            await SendMsg(result, VolumeChangedMsgType).ConfigureAwait(false);
        }

        private async void AppClosing()
        {
            await SendMsg(AppClosedMsgType).ConfigureAwait(false);
        }

        private async void AppSettingsChanged()
        {
            await AppSettingsChangedTask().ConfigureAwait(false);
        }

        private Task AppSettingsChangedTask()
        {
            var settings = new AppSettingsResponseDto
            {
                EnableHardwareAcceleration = _appSettings.EnableHardwareAcceleration,
                ForceAudioTranscode = _appSettings.ForceAudioTranscode,
                ForceVideoTranscode = _appSettings.ForceVideoTranscode,
                PlayNextFileAutomatically = _appSettings.PlayNextFileAutomatically,
                StartFilesFromTheStart = _appSettings.StartFilesFromTheStart,
                VideoScale = _appSettings.VideoScale,
            };
            return SendMsg(settings, SettingsChangedMsgType);
        }

        private async void PlayListAdded(long id)
        {
            await SendPlayLists().ConfigureAwait(false);
        }

        private async void PlayListChanged(long id)
        {
            await SendPlayLists().ConfigureAwait(false);
            await SendPlayList(id).ConfigureAwait(false);
            if (_mainViewModel.IsCurrentlyPlaying)
            {
                await FileLoadedTask().ConfigureAwait(false);
            }
        }

        private async void PlayListDeleted(long id)
        {
            await SendPlayLists().ConfigureAwait(false);
            await RefreshPlayList(id, true).ConfigureAwait(false);
            if (_mainViewModel.IsCurrentlyPlaying)
            {
                await FileLoadedTask().ConfigureAwait(false);
            }
        }

        private Task RefreshPlayList(long id, bool wasDeleted = false)
        {
            var dto = new RefreshPlayListResponseDto
            {
                Id = id,
                WasDeleted = wasDeleted
            };

            return SendMsg(dto, RefreshPlayListMsgType);
        }

        private async void FileAdded(long onPlayListId)
        {
            await SendPlayLists().ConfigureAwait(false);
            await SendPlayList(onPlayListId).ConfigureAwait(false);
        }

        private async void FileChanged(long onPlayListId)
        {
            await SendPlayList(onPlayListId).ConfigureAwait(false);
            if (_mainViewModel.IsCurrentlyPlaying)
            {
                await FileLoadedTask().ConfigureAwait(false);
            }
        }

        private async void FilesDeleted(long onPlayListId)
        {
            await SendPlayLists().ConfigureAwait(false);
            await SendPlayList(onPlayListId).ConfigureAwait(false);
            if (_mainViewModel.IsCurrentlyPlaying)
            {
                await FileLoadedTask().ConfigureAwait(false);
            }
        }

        private async void SendInfoMessage(string msg)
        {
            await SendMsg(msg, InfoMsgType).ConfigureAwait(false);
        }

        private async Task SendFileOptions(long id)
        {
            var options = _mainViewModel.GetFileOptions(id);
            await SendMsg(options, SendFileOptionsMsgType).ConfigureAwait(false);
            VolumeLevelChanged(_mainViewModel.VolumeLevel, _mainViewModel.IsMuted);
        }

        private Task SendMsg(string msgType, bool succeed = true)
        {
            var response = new EmptySocketResponseDto
            {
                MessageType = msgType,
                Succeed = succeed,
            };
            if (msgType != FilePausedMsgType)
                _logger.Info($"{nameof(SendMsg)}: Sending msg of type = {msgType}");
            return SendMsg(response);
        }

        private Task SendMsg<T>(T result, string msgType, bool succeed = true) where T : class
        {
            var response = new SocketResponseDto<T>
            {
                Succeed = succeed,
                MessageType = msgType,
                Result = result
            };
            _logger.Info($"{nameof(SendMsg)}: Sending msg of type = {msgType} with result");
            return SendMsg(response);
        }

        private Task SendMsg(EmptyResponseDto response)
        {
            string json = JsonConvert.SerializeObject(response);
            return BroadcastAsync(json);
        }
        #endregion
    }
}