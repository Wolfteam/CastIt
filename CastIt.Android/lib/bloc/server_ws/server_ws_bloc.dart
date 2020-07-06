import 'dart:async';
import 'dart:convert';

import 'package:bloc/bloc.dart';
import 'package:castit/common/enums/video_scale_type.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:web_socket_channel/io.dart';
import 'package:web_socket_channel/status.dart' as status_codes;

import '../../models/dtos/requests/play_file_request_dto.dart';
import '../../models/dtos/requests/go_to_request_dto.dart';
import '../../models/dtos/requests/app_settings_request_dto.dart';
import '../../models/dtos/requests/delete_file_request_dto.dart';
import '../../models/dtos/requests/delete_playlist_request_dto.dart';
import '../../models/dtos/requests/go_to_seconds_request_dto.dart';
import '../../models/dtos/requests/set_file_options_request_dto.dart';
import '../../models/dtos/requests/set_loop_file_request_dto.dart';
import '../../models/dtos/requests/set_playlist_options_request_dto.dart';
import '../../models/dtos/base_socket_request_dto.dart';

import '../../models/dtos/responses/app_settings_response_dto.dart';
import '../../models/dtos/responses/file_loaded_response_dto.dart';
import '../../models/dtos/socket_response_dto.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'server_ws_bloc.freezed.dart';
part 'server_ws_event.dart';
part 'server_ws_state.dart';

//TODO: MOVE ALL THE WS LOGIC HERE
class ServerWsBloc extends Bloc<ServerWsEvent, ServerWsState> {
  //Client Msg
  static const String _playMsgType = 'PLAYBLACK_PLAY';
  static const String _goToSecondsMsgType = 'PLAYBLACK_GOTO_SECONDS';
  static const String _goToMsgType = 'PLAYBLACK_GOTO';
  static const String _togglePlayBackMsgType = 'PLAYBLACK_TOGGLE';
  static const String _stopPlaybackMsgType = 'PLAYBACK_STOP';
  static const String _setPlayListOptionsMsgType = 'PLAYLIST_OPTIONS';
  static const String _deletePlayListMsgType = 'PLAYLIST_DELETE';
  static const String _deleteFileMsgType = 'FILE_DELETE';
  static const String _loopFileMsgType = 'FILE_LOOP';
  static const String _setFileOptionsMsgType = 'FILE_SET_OPTIONS';
  static const String _updateSettingsMsgType = 'SETTINGS_UPDATE';

  //Server Msg
  static const String _clientConnectedMsgType = 'CLIENT_CONNECTED';
  static const String _fileLoadingMsgType = 'FILE_LOADING';
  static const String _fileLoadedMsgType = 'FILE_LOADED';
  static const String _fileLoadingErrorMsgType = 'ERROR_ON_FILE_LOADING';
  static const String _filePositionChangedMsgType = 'FILE_POSITION_CHANGED';
  static const String _fileTimeChangedMsgType = 'FILE_TIME_CHANGED';
  static const String _filePausedMsgType = 'FILE_PAUSED';
  static const String _fileEndReachedMsgType = 'FILE_END_REACHED';
  static const String _chromeCastDisconectedMsgType = 'CHROMECAST_DISCONNECTED';
  static const String _volumeChangedMsgType = 'VOLUME_LEVEL_CHANGED';
  static const String _appClosingMsgType = 'APP_CLOSING';
  static const String _settingsChangedMsgType = 'SETTINGS_CHANGED';
  static const String _infoMsg = 'INFO_MSG';

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
  final StreamController<String> infoMsg = StreamController.broadcast();

  final SettingsService _settings;
  final LoggingService _logger;

  IOWebSocketChannel _channel;

  ServerWsBloc(this._logger, this._settings);

  @override
  ServerWsState get initialState => ServerWsState.loading();

  ServerLoadedState get currentState => state as ServerLoadedState;

  @override
  Stream<ServerWsState> mapEventToState(
    ServerWsEvent event,
  ) async* {
    final s = event.when(
      connectToWs: () {
        _connectToWs();
        return ServerWsState.loaded(
          castItUrl: _settings.castItUrl,
          connectionRetries: 0,
          isConnectedToWs: true,
        );
      },
      disconnectedFromWs: () {
        _disconnectFromWs();
        // disconnected.add(null);
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
        _connectToWs();
        return currentState.copyWith(
          isConnectedToWs: true,
          castItUrl: castitUrl,
          connectionRetries: currentState.connectionRetries + 1,
        );
      },
      showMsg: (msg) {
        return currentState.copyWith(msgToShow: msg);
      },
    );

    yield s;
  }

  //TODO: CLOSE THIS SUBSCRIPTION AND ALL THE STREAMS
  @override
  Future<void> close() {
    _disconnectFromWs();
    return super.close();
  }

  void _connectToWs() {
    try {
      final url = _settings.castItUrl;
      _disconnectFromWs();
      _channel = IOWebSocketChannel.connect(url, pingInterval: const Duration(seconds: 2));

      _channel.stream.listen((event) {
        final jsonMap = json.decode(event as String) as Map<String, dynamic>;
        _handleSocketMsg(jsonMap);
      }, onError: (e, StackTrace s) {
        //TODO: I SHOULD ENABLE THE LINE BELOW, BUT NEED TO THINK THIS CAREFULLY
        // add(ServerMsgEvent.disconnectedFromWs());
        _logger.error(runtimeType, '_connectToWs: Error while listening in channel', e, s);
      }, onDone: () {
        //TODO: I SHOULD ENABLE THE LINE BELOW, BUT NEED TO THINK THIS CAREFULLY
        // add(ServerMsgEvent.disconnectedFromWs());
        _logger.info(runtimeType, '_connectToWs: Disconnected from ws');
      });
    } catch (e, s) {
      add(ServerWsEvent.disconnectedFromWs());
      _logger.error(runtimeType, '_connectToWs: Unknown error', e, s);
    }
  }

  void _disconnectFromWs() {
    if (_channel != null) {
      _channel.sink.close(status_codes.goingAway, 'Disconnected');
    }

    _channel = null;
    disconnected.add(null);
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
        fileLoading.add(null);
        break;
      case _fileLoadedMsgType:
        final file = response.result as FileLoadedResponseDto;
        fileLoaded.add(file);
        break;
      case _fileLoadingErrorMsgType:
        fileLoadingError.add('Error loading file');
        break;
      case _filePositionChangedMsgType:
        // final filePositionChanged = SocketResponseDto.fromJson(json);
        break;
      case _fileTimeChangedMsgType:
        fileTimeChanged.add(response.result as double);
        break;
      case _volumeChangedMsgType:
        // final volumeChanged = SocketResponseDto.fromJson(json);
        break;
      case _fileEndReachedMsgType:
        fileEndReached.add(null);
        break;
      case _filePausedMsgType:
        filePaused.add(null);
        break;
      case _chromeCastDisconectedMsgType:
        disconnected.add(null);
        add(ServerWsEvent.connectToWs());
        break;
      case _appClosingMsgType:
        appClosing.add(null);
        disconnected.add(null);
        add(ServerWsEvent.disconnectedFromWs());
        break;
      case _settingsChangedMsgType:
        final settings = response.result as AppSettingsResponseDto;
        settingsChanged.add(settings);
        break;
      case _infoMsg:
        final msg = response.result as String;
        infoMsg.add(msg);
        break;
      default:
        _logger.warning(
          runtimeType,
          '_handleSocketMsg: Msg = $msgType is not being handled',
        );
        break;
    }
  }

  void _sendMsg(dynamic dto) {
    try {
      final msg = json.encode(dto);
      _channel.sink.add(msg);
    } catch (e, s) {
      _logger.error(runtimeType, '_sendMsg: Unknown errror', e, s);
    }
  }

  void playFile(int id, int playListId) {
    final dto = PlayFileRequestDto(msgType: _playMsgType, id: id, playListId: playListId);
    _sendMsg(dto);
  }

  void gotoSeconds(double seconds) {
    final dto = GoToSecondsRequestDto(msgType: _goToSecondsMsgType, seconds: seconds);
    _sendMsg(dto);
  }

  void goTo({bool next = false, bool previous = false}) {
    final dto = GoToRequestDto(msgType: _goToMsgType, next: next, previous: previous);
    _sendMsg(dto);
  }

  void togglePlayBack() {
    final dto = BaseSocketRequestDto(messageType: _togglePlayBackMsgType);
    _sendMsg(dto);
  }

  void stopPlayBack() {
    final dto = BaseSocketRequestDto(messageType: _stopPlaybackMsgType);
    _sendMsg(dto);
  }

  void setPlayListOptions(int id, {bool loop = false, bool shuffle = false}) {
    final dto = SetPlayListOptionsRequestDto(msgType: _setPlayListOptionsMsgType, id: id, loop: loop, shuffle: shuffle);
    _sendMsg(dto);
  }

  void deletePlayList(int id) {
    final dto = DeletePlayListRequestDto(msgType: _deletePlayListMsgType, id: id);
    _sendMsg(dto);
  }

  void deleteFile(int id, int playListId) {
    final dto = DeleteFileRequestDto(msgType: _deleteFileMsgType, id: id, playListId: playListId);
    _sendMsg(dto);
  }

  void loopFile(int id, int playListId, {bool loop = false}) {
    final dto = SetLoopFileRequestDto(msgType: _loopFileMsgType, id: id, playListId: playListId, loop: loop);
    _sendMsg(dto);
  }

  void setFileOptions(int streamIndex, {bool isAudio = false, bool isSubtitle = false, bool isQuality = false}) {
    final dto = SetFileOptionsRequestDto(
      msgType: _setFileOptionsMsgType,
      streamIndex: streamIndex,
      isAudio: isAudio,
      isQuality: isQuality,
      isSubTitle: isSubtitle,
    );
    _sendMsg(dto);
  }

  void updateSettings({
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
    _sendMsg(dto);
  }
}
