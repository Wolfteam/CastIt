import 'dart:async';
import 'dart:convert';
import 'dart:io';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:web_socket_channel/io.dart';
import 'package:web_socket_channel/status.dart' as status_codes;

import '../../common/enums/video_scale_type.dart';
import '../../models/dtos/dtos.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'server_ws_bloc.freezed.dart';
part 'server_ws_event.dart';
part 'server_ws_state.dart';

//TODO: MOVE ALL THE WS LOGIC HERE
class ServerWsBloc extends Bloc<ServerWsEvent, ServerWsState> {
  //Client Msg
  static const String _getPlayListsMsgType = 'CLIENT_PLAYLISTS_ALL';
  static const String _getPlayListMsgType = 'CLIENT_PLAYLIST_ONE';
  static const String _playMsgType = 'CLIENT_PLAYBLACK_PLAY';
  static const String _goToSecondsMsgType = 'CLIENT_PLAYBLACK_GOTO_SECONDS';
  static const String _skipSecondsMsgType = 'CLIENT_PLAYBLACK_SKIP_SECONDS';
  static const String _goToMsgType = 'CLIENT_PLAYBLACK_GOTO';
  static const String _togglePlayBackMsgType = 'CLIENT_PLAYBLACK_TOGGLE';
  static const String _stopPlaybackMsgType = 'CLIENT_PLAYBACK_STOP';
  static const String _setPlayListOptionsMsgType = 'CLIENT_PLAYLIST_OPTIONS';
  static const String _deletePlayListMsgType = 'CLIENT_PLAYLIST_DELETE';
  static const String _renamePlayListMsgType = 'CLIENT_PLAYLIST_RENAME';
  static const String _deleteFileMsgType = 'CLIENT_FILE_DELETE';
  static const String _loopFileMsgType = 'CLIENT_FILE_LOOP';
  static const String _setFileOptionsMsgType = 'CLIENT_FILE_SET_OPTIONS';
  static const String _getFileOptionsMsgType = 'CLIENT_GET_FILE_OPTIONS';
  static const String _updateSettingsMsgType = 'CLIENT_SETTINGS_UPDATE';
  static const String _setVolumeMsgType = 'CLIENT_SET_VOLUME';

  //Server Msg
  static const String _gotPlayListsMsgType = 'SERVER_PLAYLISTS_ALL';
  static const String _gotPlayListMsgType = 'SERVER_PLAYLISTS_ONE';
  static const String _refreshPlayListMsgType = 'SERVER_PLAYLIST_REFRESH';
  static const String _clientConnectedMsgType = 'SERVER_CLIENT_CONNECTED';
  static const String _fileLoadingMsgType = 'SERVER_FILE_LOADING';
  static const String _fileLoadedMsgType = 'SERVER_FILE_LOADED';
  static const String _fileLoadingErrorMsgType = 'SERVER_ERROR_ON_FILE_LOADING';
  static const String _filePositionChangedMsgType = 'SERVER_FILE_POSITION_CHANGED';
  static const String _fileTimeChangedMsgType = 'SERVER_FILE_TIME_CHANGED';
  static const String _filePausedMsgType = 'SERVER_FILE_PAUSED';
  static const String _fileEndReachedMsgType = 'SERVER_FILE_END_REACHED';
  static const String _sendFileOptionsMsgType = 'SERVER_SEND_FILE_OPTIONS';
  static const String _chromeCastDisconectedMsgType = 'SERVER_CHROMECAST_DISCONNECTED';
  static const String _volumeChangedMsgType = 'SERVER_VOLUME_LEVEL_CHANGED';
  static const String _appClosingMsgType = 'SERVER_APP_CLOSING';
  static const String _settingsChangedMsgType = 'SERVER_SETTINGS_CHANGED';
  static const String _infoMsg = 'SERVER_INFO_MSG';

  final StreamController<void> connected = StreamController.broadcast();
  final StreamController<void> fileLoading = StreamController.broadcast();
  final StreamController<FileLoadedResponseDto> fileLoaded = StreamController.broadcast();
  final StreamController<String> fileLoadingError = StreamController.broadcast();
  final StreamController<double> fileTimeChanged = StreamController.broadcast();
  final StreamController<void> filePaused = StreamController.broadcast();
  final StreamController<void> fileEndReached = StreamController.broadcast();
  final StreamController<void> disconnected = StreamController.broadcast();
  final StreamController<void> appClosing = StreamController.broadcast();
  final StreamController<AppSettingsResponseDto> settingsChanged = StreamController.broadcast();
  final StreamController<List<GetAllPlayListResponseDto>> playlistsLoaded = StreamController.broadcast();
  final StreamController<PlayListItemResponseDto> playlistLoaded = StreamController.broadcast();
  final StreamController<List<FileItemOptionsResponseDto>> fileOptionsLoaded = StreamController.broadcast();
  final StreamController<VolumeLevelChangedResponseDto> volumeLevelChanged = StreamController.broadcast();
  final StreamController<RefreshPlayListResponseDto> refreshPlayList = StreamController.broadcast();

  final SettingsService _settings;
  final LoggingService _logger;

  bool isServerRunning = false;

  IOWebSocketChannel _channel;

  ServerWsState get initialState => ServerWsState.loading();

  ServerLoadedState get currentState => state as ServerLoadedState;

  ServerWsBloc(this._logger, this._settings) : super(ServerWsState.loading());

  @override
  Stream<ServerWsState> mapEventToState(
    ServerWsEvent event,
  ) async* {
    await _isServerRunning();

    final s = event.when(
      connectToWs: () {
        if (isServerRunning) {
          _connectToWs();
        }
        return ServerWsState.loaded(
          castItUrl: _settings.castItUrl,
          connectionRetries: 0,
          isConnectedToWs: isServerRunning,
        );
      },
      disconnectedFromWs: () {
        _disconnectFromWs(triggerEvent: false);
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries + 1,
        );
      },
      disconnectFromWs: () {
        _disconnectFromWs();
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries + 1,
        );
      },
      updateUrlAndConnectToWs: (castitUrl) {
        _settings.castItUrl = castitUrl;
        if (isServerRunning) {
          _connectToWs();
        }
        return currentState.copyWith(
          isConnectedToWs: isServerRunning,
          castItUrl: castitUrl,
          connectionRetries: currentState.connectionRetries + 1,
        );
      },
      showMsg: (msg) {
        return currentState.copyWith(msgToShow: msg);
      },
    );

    yield s;
    if (currentState.msgToShow != null) {
      yield currentState.copyWith(msgToShow: null);
    }
  }

  //TODO: CLOSE THIS SUBSCRIPTION AND ALL THE STREAMS
  @override
  Future<void> close() {
    _disconnectFromWs();
    return super.close();
  }

  String _getWsUrl() {
    final url = _cleanUrl();
    return 'ws://$url/socket';
  }

  String _cleanUrl() {
    String url = _settings.castItUrl;
    url = url.replaceFirst('http://', '');
    url = url.replaceFirst('https://', '');

    return url;
  }

  Future<bool> _isServerRunning() async {
    final url = _cleanUrl();
    try {
      final uri = Uri.http(url, '');
      final socket = await Socket.connect(uri.host, uri.port, timeout: const Duration(seconds: 1));
      socket.destroy();
      _logger.info(runtimeType, '_isServerRunning: Server is running');
      isServerRunning = true;
    } catch (e) {
      _logger.warning(
        runtimeType,
        '_isServerRunning: Connectivity error, server may not be running or url = $url is not valid',
      );
      isServerRunning = false;
    }
    return isServerRunning;
  }

  void _connectToWs() {
    try {
      _disconnectFromWs(triggerEvent: false);
      final url = _getWsUrl();
      _channel = IOWebSocketChannel.connect(url, pingInterval: const Duration(seconds: 2));

      _channel.stream.listen((event) {
        final jsonMap = json.decode(event as String) as Map<String, dynamic>;
        _handleSocketMsg(jsonMap);
      }, onError: (e, StackTrace s) async {
        //TODO: I SHOULD ENABLE THE LINE BELOW, BUT NEED TO THINK THIS CAREFULLY
        await _onWsErrorDone(true);
        _logger.error(runtimeType, '_connectToWs: Error while listening in channel', e, s);
      }, onDone: () async {
        //TODO: I SHOULD ENABLE THE LINE BELOW, BUT NEED TO THINK THIS CAREFULLY
        await _onWsErrorDone(false);
        _logger.info(runtimeType, '_connectToWs: Disconnected from ws');
      });
    } catch (e, s) {
      _disconnectFromWs();
      _logger.error(runtimeType, '_connectToWs: Unknown error', e, s);
    }
  }

  void _disconnectFromWs({bool triggerEvent = true}) {
    if (_channel != null) {
      _channel.sink.close(status_codes.goingAway, 'Disconnected');
    }

    // isServerRunning = false;
    _channel = null;
    if (triggerEvent) {
      disconnected.add(null);
    }
  }

  void _handleSocketMsg(Map<String, dynamic> json) {
    if (!json.containsKey('MessageType')) return;
    final msgType = json['MessageType'] as String;
    final response = SocketResponseDto.fromJson(json);
    switch (msgType) {
      case _clientConnectedMsgType:
        connected.add(null);
        break;
      case _fileLoadingMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: A file is loading...');
        fileLoading.add(null);
        break;
      case _fileLoadedMsgType:
        final file = response.result as FileLoadedResponseDto;
        _logger.info(runtimeType, '_handleSocketMsg: File = ${file.filename} was loaded');
        fileLoaded.add(file);
        break;
      case _fileLoadingErrorMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Error loading file');
        fileLoadingError.add('Error loading file');
        break;
      case _filePositionChangedMsgType:
        // final filePositionChanged = SocketResponseDto.fromJson(json);
        break;
      case _fileTimeChangedMsgType:
        fileTimeChanged.add(response.result as double);
        break;
      case _volumeChangedMsgType:
        final volumeChanged = response.result as VolumeLevelChangedResponseDto;
        volumeLevelChanged.add(volumeChanged);
        break;
      case _fileEndReachedMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: File end reached');
        fileEndReached.add(null);
        break;
      case _filePausedMsgType:
        filePaused.add(null);
        break;
      case _chromeCastDisconectedMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Chromecast is disconnected');
        disconnected.add(null);
        add(ServerWsEvent.connectToWs());
        break;
      case _appClosingMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Desktop app is being closed');
        appClosing.add(null);
        _disconnectFromWs();
        isServerRunning = false;
        break;
      case _settingsChangedMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Settings were loaded');
        final settings = response.result as AppSettingsResponseDto;
        settingsChanged.add(settings);
        break;
      case _infoMsg:
        final msg = response.result as String;
        _logger.info(runtimeType, '_handleSocketMsg: Server msg received = $msg');
        add(ServerWsEvent.showMsg(msg: msg));
        break;
      case _gotPlayListsMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Playlists loaded');
        final pls = response.result as List<dynamic>;
        playlistsLoaded.add(pls.map((e) => e as GetAllPlayListResponseDto).toList());
        break;
      case _gotPlayListMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: Playlist loaded');
        final pl = response.result as PlayListItemResponseDto;
        playlistLoaded.add(pl);
        break;
      case _sendFileOptionsMsgType:
        _logger.info(runtimeType, '_handleSocketMsg: File options loaded');
        final fo = response.result as List<dynamic>;
        fileOptionsLoaded.add(fo.map((e) => e as FileItemOptionsResponseDto).toList());
        break;
      case _refreshPlayListMsgType:
        final refreshDto = response.result as RefreshPlayListResponseDto;
        _logger.info(runtimeType, '_handleSocketMsg: Refreshing playlistId = ${refreshDto.id}');
        refreshPlayList.add(refreshDto);
        break;
      default:
        _logger.warning(
          runtimeType,
          '_handleSocketMsg: Msg = $msgType is not being handled',
        );
        break;
    }
  }

  Future<void> _onWsErrorDone(bool error) async {
    await _isServerRunning();
    if (!isServerRunning) {
      _disconnectFromWs();
    }
  }

  Future<void> _sendMsg(BaseSocketRequestDto dto) async {
    try {
      _logger.info(runtimeType, '_sendMsg: Trying to send msgType = ${dto.messageType}');
      final msg = json.encode(dto);
      if (!await _isServerRunning()) {
        _disconnectFromWs();
        return;
      }
      if (_channel == null) {
        _connectToWs();
      }
      _channel.sink.add(msg);
    } catch (e, s) {
      _disconnectFromWs();
      _logger.error(runtimeType, '_sendMsg: Unknown errror', e, s);
    }
  }

  Future<void> playFile(int id, int playListId, {bool force = false}) {
    final dto = PlayFileRequestDto(msgType: _playMsgType, id: id, playListId: playListId, force: force);
    return _sendMsg(dto);
  }

  Future<void> gotoSeconds(double seconds) {
    final dto = GoToSecondsRequestDto(msgType: _goToSecondsMsgType, seconds: seconds);
    return _sendMsg(dto);
  }

  Future<void> skipSeconds(double seconds) {
    final dto = GoToSecondsRequestDto(msgType: _skipSecondsMsgType, seconds: seconds);
    return _sendMsg(dto);
  }

  Future<void> goTo({bool next = false, bool previous = false}) {
    final dto = GoToRequestDto(msgType: _goToMsgType, next: next, previous: previous);
    return _sendMsg(dto);
  }

  Future<void> togglePlayBack() {
    final dto = BaseSocketRequestDto(messageType: _togglePlayBackMsgType);
    return _sendMsg(dto);
  }

  Future<void> stopPlayBack() {
    final dto = BaseSocketRequestDto(messageType: _stopPlaybackMsgType);
    return _sendMsg(dto);
  }

  Future<void> setPlayListOptions(int id, {bool loop = false, bool shuffle = false}) {
    final dto = SetPlayListOptionsRequestDto(msgType: _setPlayListOptionsMsgType, id: id, loop: loop, shuffle: shuffle);
    return _sendMsg(dto);
  }

  Future<void> deletePlayList(int id) {
    final dto = DeletePlayListRequestDto(msgType: _deletePlayListMsgType, id: id);
    return _sendMsg(dto);
  }

  Future<void> deleteFile(int id, int playListId) {
    final dto = DeleteFileRequestDto(msgType: _deleteFileMsgType, id: id, playListId: playListId);
    return _sendMsg(dto);
  }

  Future<void> loopFile(int id, int playListId, {bool loop = false}) {
    final dto = SetLoopFileRequestDto(msgType: _loopFileMsgType, id: id, playListId: playListId, loop: loop);
    return _sendMsg(dto);
  }

  Future<void> setFileOptions(
    int streamIndex, {
    bool isAudio = false,
    bool isSubtitle = false,
    bool isQuality = false,
  }) {
    final dto = SetFileOptionsRequestDto(
      msgType: _setFileOptionsMsgType,
      streamIndex: streamIndex,
      isAudio: isAudio,
      isQuality: isQuality,
      isSubTitle: isSubtitle,
    );
    return _sendMsg(dto);
  }

  Future<void> updateSettings({
    VideoScaleType videoScale = VideoScaleType.original,
    bool playFromTheStart = false,
    bool playNextFileAutomatically = false,
    bool forceAudioTranscode = false,
    bool forceVideoTranscode = false,
    bool enableHwAccel = false,
  }) {
    final dto = AppSettingsRequestDto(
      msgType: _updateSettingsMsgType,
      enableHwAccel: enableHwAccel,
      forceAudioTranscode: forceAudioTranscode,
      forceVideoTranscode: forceVideoTranscode,
      playFromTheStart: playFromTheStart,
      playNextFileAutomatically: playNextFileAutomatically,
      videoScale: getVideoScaleValue(videoScale),
    );
    return _sendMsg(dto);
  }

  Future<void> loadPlayLists() {
    final dto = BaseSocketRequestDto(messageType: _getPlayListsMsgType);
    return _sendMsg(dto);
  }

  Future<void> loadPlayList(int playListId) {
    final dto = BaseItemRequestDto(id: playListId, msgType: _getPlayListMsgType);
    return _sendMsg(dto);
  }

  Future<void> loadFileOptions(int id) {
    final dto = BaseItemRequestDto(id: id, msgType: _getFileOptionsMsgType);
    return _sendMsg(dto);
  }

  Future<void> setVolume(double volumeLvl, bool isMuted) {
    final dto = SetVolumeRequestDto(volumeLevel: volumeLvl, isMuted: isMuted, msgType: _setVolumeMsgType);
    return _sendMsg(dto);
  }

  Future<void> renamePlayList(int id, String name) {
    final dto = RenamePlayListRequestDto(id: id, name: name, msgType: _renamePlayListMsgType);
    return _sendMsg(dto);
  }
}
