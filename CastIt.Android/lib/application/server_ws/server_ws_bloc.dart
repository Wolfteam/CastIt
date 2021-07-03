import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:signalr_core/signalr_core.dart';
import 'package:tuple/tuple.dart';

part 'server_ws_bloc.freezed.dart';
part 'server_ws_event.dart';
part 'server_ws_state.dart';

class ServerWsBloc extends Bloc<ServerWsEvent, ServerWsState> {
  static const String _sendPlayLists = 'SendPlayLists';
  static const String _stoppedPlayback = 'StoppedPlayBack';
  static const String _playListAdded = 'PlayListAdded';
  static const String _playListChanged = 'PlayListChanged';
  static const String _playListsChanged = 'PlayListsChanged';
  static const String _playListDeleted = 'PlayListDeleted';
  static const String _playListBusy = 'PlayListIsBusy';
  static const String _fileAdded = 'FileAdded';
  static const String _fileChanged = 'FileChanged';
  static const String _filesChanged = 'FilesChanged';
  static const String _fileDeleted = 'FileDeleted';
  static const String _fileLoading = 'FileLoading';
  static const String _fileLoaded = 'FileLoaded';
  static const String _fileEndReached = 'FileEndReached';
  static const String _playerStatusChanged = 'PlayerStatusChanged';
  static const String _playerSettingsChanged = 'PlayerSettingsChanged';
  static const String _serverMessage = 'ServerMessage';
  static const String _castDeviceSet = 'CastDeviceSet';
  static const String _castDevicesChanged = 'CastDevicesChanged';
  static const String _castDeviceDisconnected = 'CastDeviceDisconnected';

  final StreamController<void> connected = StreamController.broadcast();
  final StreamController<void> fileLoading = StreamController.broadcast();
  final StreamController<PlayedFile> fileLoaded = StreamController.broadcast();
  final StreamController<String> fileLoadingError = StreamController.broadcast();
  final StreamController<double> fileTimeChanged = StreamController.broadcast();
  final StreamController<void> filePaused = StreamController.broadcast();
  final StreamController<void> fileEndReached = StreamController.broadcast();
  final StreamController<void> disconnected = StreamController.broadcast();
  final StreamController<void> appClosing = StreamController.broadcast();
  final StreamController<ServerAppSettings> settingsChanged = StreamController.broadcast();
  final StreamController<PlayListItemResponseDto?> playlistLoaded = StreamController.broadcast();
  final StreamController<List<FileItemOptionsResponseDto>> fileOptionsLoaded = StreamController.broadcast();
  final StreamController<VolumeLevelChangedResponseDto> volumeLevelChanged = StreamController.broadcast();
  final StreamController<RefreshPlayListResponseDto> refreshPlayList = StreamController.broadcast();

  final StreamController<GetAllPlayListResponseDto> playListAdded = StreamController.broadcast();
  final StreamController<List<GetAllPlayListResponseDto>> playListsChanged = StreamController.broadcast();
  final StreamController<Tuple2<bool, GetAllPlayListResponseDto>> playListChanged = StreamController.broadcast();
  final StreamController<int> playListDeleted = StreamController.broadcast();

  final StreamController<FileItemResponseDto> fileAdded = StreamController.broadcast();
  final StreamController<Tuple2<bool, FileItemResponseDto>> fileChanged = StreamController.broadcast();
  final StreamController<List<FileItemResponseDto>> filesChanged = StreamController.broadcast();
  final StreamController<Tuple2<int, int>> fileDeleted = StreamController.broadcast();

  final SettingsService _settings;
  final LoggingService _logger;
  HubConnection? _connection;

  bool get isServerRunning => _connection?.state == HubConnectionState.connected;

  ServerWsState get initialState => ServerWsState.loading();

  _LoadedState get currentState => state as _LoadedState;

  ServerWsBloc(this._logger, this._settings) : super(ServerWsState.loading());

  @override
  Stream<ServerWsState> mapEventToState(ServerWsEvent event) async* {
    // await _isServerRunning();

    if (event is _Disconnected && isServerRunning) {
      _logger.info(runtimeType, 'A server disconnected from ws event was raised but the server is running');
      return;
    }
    _logger.info(runtimeType, 'Handling event = $event');
    final s = event.when(
      connectToWs: () async {
        if (!isServerRunning) {
          await _connectToHub();
        }
        return ServerWsState.loaded(
          castItUrl: _settings.castItUrl,
          connectionRetries: 0,
          isConnectedToWs: isServerRunning,
        );
      },
      disconnectedFromWs: () async {
        await _disconnectFromHub(triggerEvent: false);
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries! + 1,
        );
      },
      disconnectFromWs: () async {
        await _disconnectFromHub();
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries! + 1,
        );
      },
      updateUrlAndConnectToWs: (newUrl) async {
        final changed = _settings.castItUrl != newUrl;
        _settings.castItUrl = newUrl;
        if (changed) {
          await _connectToHub();
        }
        return currentState.copyWith(
          isConnectedToWs: isServerRunning,
          castItUrl: newUrl,
          connectionRetries: currentState.connectionRetries! + 1,
        );
      },
      showMsg: (msg) async {
        return currentState.copyWith(msgToShow: msg);
      },
    );

    yield await s;
    if (currentState.msgToShow != null) {
      yield currentState.copyWith(msgToShow: null);
    }
  }

  @override
  Future<void> close() async {
    await _disconnectFromHub();
    await Future.wait([
      connected.close(),
      fileLoading.close(),
      fileLoaded.close(),
      fileLoadingError.close(),
      fileTimeChanged.close(),
      filePaused.close(),
      fileEndReached.close(),
      disconnected.close(),
      appClosing.close(),
      settingsChanged.close(),
      playListsChanged.close(),
      playlistLoaded.close(),
      fileOptionsLoaded.close(),
      volumeLevelChanged.close(),
      refreshPlayList.close(),
    ]);

    await super.close();
  }

  // Future<bool> _isServerRunning() async {
  //   final url = _settings.castItUrl;
  //   try {
  //     final uri = Uri.parse(url);
  //     final socket = await Socket.connect(uri.host, uri.port, timeout: const Duration(seconds: 1));
  //     socket.destroy();
  //     _logger.info(runtimeType, '_isServerRunning: Server is running');
  //     isServerRunning = true;
  //   } catch (e, s) {
  //     _logger.error(runtimeType, '_isServerRunning: Connectivity error, server may not be running or url = $url is not valid', e, s);
  //     isServerRunning = false;
  //   }
  //   return isServerRunning;
  // }

  Future<void> _connectToHub() async {
    final url = '${_settings.castItUrl}/castithub';
    try {
      _logger.info(runtimeType, 'Trying to connect to hub on = $url ...');
      final options = HttpConnectionOptions(
        logging: (level, message) {
          switch (level) {
            case LogLevel.trace:
            case LogLevel.debug:
            case LogLevel.none:
            case LogLevel.information:
              // _logger.info(runtimeType, message);
              break;
            case LogLevel.warning:
              // _logger.warning(runtimeType, message);
              break;
            case LogLevel.error:
            case LogLevel.critical:
              _logger.error(runtimeType, message);
              break;
          }
        },
      );

      await _disconnectFromHub(triggerEvent: false);

      _connection = HubConnectionBuilder().withUrl(url, options).build();
      _listenHubEvents(_connection!);
      await _connection!.start();
    } catch (e, s) {
      _logger.error(runtimeType, '_connectToHub: Error while trying to connect to hub = $url', e, s);
      await _disconnectFromHub();
    }
  }

  Future<void> _disconnectFromHub({bool triggerEvent = true}) async {
    if (_connection != null) {
      _connection!.off(_sendPlayLists);

      _connection!.off(_playListAdded);

      _connection!.off(_playListChanged);

      _connection!.off(_playListsChanged);

      _connection!.off(_playListDeleted);

      _connection!.off(_playListBusy);

      _connection!.off(_fileAdded);

      _connection!.off(_fileChanged);

      _connection!.off(_filesChanged);

      _connection!.off(_fileDeleted);

      _connection!.off(_fileLoading);

      _connection!.off(_fileLoaded);

      _connection!.off(_fileEndReached);

      _connection!.off(_playerStatusChanged);

      _connection!.off(_playerSettingsChanged);

      _connection!.off(_serverMessage);

      _connection!.off(_castDeviceSet);

      _connection!.off(_castDevicesChanged);

      _connection!.off(_castDeviceDisconnected);

      await _connection!.stop();
    }

    _connection = null;
    if (triggerEvent) {
      disconnected.add(null);
    }

    // isServerRunning = false;
  }

  void _listenHubEvents(HubConnection connection) {
    connection.onclose((exception) async {
      if (exception != null) {
        await _onHubErrorOrDone(false);
        _logger.info(runtimeType, '_listenHubEvents: Channel done method was called');
      } else {
        await _onHubErrorOrDone(false);
        _logger.info(runtimeType, '_listenHubEvents: Channel done method was called');
      }
    });
    connection.on(_sendPlayLists, (message) => _handleHubMsg(_sendPlayLists, message?.first));

    //TODO: IT SEEMS METHODS WITHOUT PARAMS FAILS WITH THIS LIB
    // connection.on(_stoppedPlayback, (message) => _handleHubMsg(_stoppedPlayback, message?.first));

    connection.on(_playListAdded, (message) => _handleHubMsg(_playListAdded, message?.first));

    connection.on(_playListChanged, (message) => _handleHubMsg(_playListChanged, message?.first));

    connection.on(_playListsChanged, (message) => _handleHubMsg(_playListsChanged, message?.first));

    connection.on(_playListDeleted, (message) => _handleHubMsg(_playListDeleted, message?.first));

    connection.on(_playListBusy, (message) => _handleHubMsg(_playListBusy, message?.first));

    connection.on(_fileAdded, (message) => _handleHubMsg(_fileAdded, message?.first));

    connection.on(_fileChanged, (message) => _handleHubMsg(_fileChanged, message?.first));

    connection.on(_filesChanged, (message) => _handleHubMsg(_filesChanged, message?.first));

    connection.on(_fileDeleted, (message) => _handleHubMsg(_fileDeleted, message));

    connection.on(_fileLoading, (message) => _handleHubMsg(_fileLoading, message?.first));

    connection.on(_fileLoaded, (message) => _handleHubMsg(_fileLoaded, message?.first));

    connection.on(_fileEndReached, (message) => _handleHubMsg(_fileEndReached, message?.first));

    connection.on(_playerStatusChanged, (message) => _handleHubMsg(_playerStatusChanged, message?.first));

    connection.on(_playerSettingsChanged, (message) => _handleHubMsg(_playerSettingsChanged, message?.first));

    connection.on(_serverMessage, (message) => _handleHubMsg(_serverMessage, message?.first));

    connection.on(_castDeviceSet, (message) => _handleHubMsg(_castDeviceSet, message?.first));

    connection.on(_castDevicesChanged, (message) => _handleHubMsg(_castDevicesChanged, message?.first));

    connection.on(_castDeviceDisconnected, (message) => _handleHubMsg(_castDeviceDisconnected, message?.first));
  }

  void _handleHubMsg(String msgType, dynamic message) {
    if (msgType != _playerStatusChanged) {
      _logger.info(runtimeType, 'Handling msgType = $msgType');
    }
    switch (msgType) {
      case _sendPlayLists:
        final pls = message as List<dynamic>;
        final playlists = pls.map((e) => GetAllPlayListResponseDto.fromJson(e as Map<String, dynamic>)).toList();
        playListsChanged.add(playlists);
        break;
      case _playerSettingsChanged:
        _logger.info(runtimeType, '_handleSocketMsg: Settings were loaded');
        final settings = ServerAppSettings.fromJson(message as Map<String, dynamic>);
        connected.add(null);
        settingsChanged.add(settings);
        break;
      case _playerStatusChanged:
        final status = ServerPlayerStatusResponseDto.fromJson(message as Map<String, dynamic>);
        if (status.player.isPaused) {
          filePaused.add(null);
        }
        if (status.playedFile != null) {
          playListChanged.add(Tuple2(true, status.playList!));
          final f = PlayedFile.from(status);
          fileLoaded.add(f);
          fileChanged.add(Tuple2(true, status.playedFile!));
          fileOptionsLoaded.add(status.playedFile!.streams);
          fileTimeChanged.add(f.currentSeconds);
        } else {
          fileEndReached.add(null);
        }
        final volume = VolumeLevelChangedResponseDto(isMuted: status.player.isMuted, volumeLevel: status.player.volumeLevel);
        volumeLevelChanged.add(volume);
        break;
      case _playListAdded:
        final newPl = GetAllPlayListResponseDto.fromJson(message as Map<String, dynamic>);
        playListAdded.add(newPl);
        break;
      case _playListChanged:
        final updatedPl = GetAllPlayListResponseDto.fromJson(message as Map<String, dynamic>);
        playListChanged.add(Tuple2(false, updatedPl));
        break;
      case _playListsChanged:
        final playLists = (message as List<dynamic>).map((e) => GetAllPlayListResponseDto.fromJson(e as Map<String, dynamic>)).toList();
        playListsChanged.add(playLists);
        break;
      case _playListDeleted:
        final id = message as int;
        playListDeleted.add(id);
        break;
      case _playListBusy:
        break;
      case _fileAdded:
        final file = FileItemResponseDto.fromJson(message as Map<String, dynamic>);
        fileAdded.add(file);
        break;
      case _fileChanged:
        final file = FileItemResponseDto.fromJson(message as Map<String, dynamic>);
        fileChanged.add(Tuple2(false, file));
        break;
      case _filesChanged:
        final files = (message as List<dynamic>).map((e) => FileItemResponseDto.fromJson(e as Map<String, dynamic>)).toList();
        filesChanged.add(files);
        break;
      case _fileDeleted:
        final items = message as List<dynamic>;
        final playListId = items.first as int;
        final fileId = items.last as int;
        fileDeleted.add(Tuple2(playListId, fileId));
        break;
      case _fileLoading:
        _logger.info(runtimeType, '_handleSocketMsg: A file is loading...');
        fileLoading.add(null);
        break;
      case _serverMessage:
        final code = message as int;
        add(ServerWsEvent.showMsg(type: getAppMessageType(code)));
        break;
      case _stoppedPlayback:
      case _fileEndReached:
        _logger.info(runtimeType, '_handleSocketMsg: File end reached');
        fileEndReached.add(null);
        break;
      case _castDevicesChanged:
        break;
      case _castDeviceSet:
        break;
      case _castDeviceDisconnected:
        break;
      case _fileLoaded:
        break;
      default:
        _logger.warning(runtimeType, '_handleSocketMsg: Msg = $msgType is not being handled');
        break;
    }
  }

  Future<void> _onHubErrorOrDone(bool error) async {
    // await _isServerRunning();
    if (!isServerRunning) {
      _disconnectFromHub();
    }
  }

  Future<dynamic> _invokeHubMethod(String methodName, List<dynamic> args) async {
    try {
      _logger.info(runtimeType, '_invokeHubMethod: Trying to call method  = $methodName');
      if (_connection == null) {
        _logger.info(runtimeType, '_invokeHubMethod: Connection is null, trying to establish connection before calling method = $methodName');
        await _connectToHub();
      }

      if (!isServerRunning) {
        _logger.warning(runtimeType, '_invokeHubMethod: Cannot invoke method  = $methodName cause the server is not running');
        await _disconnectFromHub();
        return;
      }

      return await _connection!.invoke(methodName, args: args);
    } catch (e, s) {
      await _disconnectFromHub();
      _logger.error(runtimeType, '_invokeHubMethod: Unknown error', e, s);
    }
    return null;
  }

  Future<void> playFile(int id, int playListId, {bool force = false}) async {
    final dto = PlayFileRequestDto(id: id, playListId: playListId, force: force);
    await _invokeHubMethod('Play', [dto]);
  }

  Future<void> gotoSeconds(double seconds) async {
    await _invokeHubMethod('GoToSeconds', [seconds]);
  }

  Future<void> skipSeconds(double seconds) async {
    await _invokeHubMethod('SkipSeconds', [seconds]);
  }

  Future<void> goTo({bool next = false, bool previous = false}) async {
    await _invokeHubMethod('GoTo', [next, previous]);
  }

  Future<void> togglePlayBack() async {
    await _invokeHubMethod('TogglePlayBack', []);
  }

  Future<void> stopPlayBack() async {
    await _invokeHubMethod('StopPlayback', []);
  }

  Future<void> setPlayListOptions(int id, {bool loop = false, bool shuffle = false}) async {
    final dto = SetPlayListOptionsRequestDto(loop: loop, shuffle: shuffle);
    await _invokeHubMethod('SetPlayListOptions', [id, dto]);
  }

  Future<void> deletePlayList(int id) async {
    await _invokeHubMethod('DeletePlayList', [id]);
  }

  Future<void> deleteFile(int id, int playListId) async {
    await _invokeHubMethod('DeleteFile', [playListId, id]);
  }

  Future<void> loopFile(int id, int playListId, {bool loop = false}) async {
    await _invokeHubMethod('LoopFile', [playListId, id, loop]);
  }

  Future<void> setFileOptions(int streamIndex, {bool isAudio = false, bool isSubtitle = false, bool isQuality = false}) async {
    final dto = SetFileOptionsRequestDto(streamIndex: streamIndex, isAudio: isAudio, isQuality: isQuality, isSubTitle: isSubtitle);
    await _invokeHubMethod('SetFileOptions', [dto]);
  }

  Future<void> updateSettings(ServerAppSettings dto) async {
    await _invokeHubMethod('UpdateSettings', [dto]);
  }

  Future<void> loadPlayLists() async {
    await _invokeHubMethod('SendPlayListsToClient', []);
  }

  Future<PlayListItemResponseDto> loadPlayList(int playListId) async {
    final response = await _invokeHubMethod('GetPlayList', [playListId]);
    return PlayListItemResponseDto.fromJson(response as Map<String, dynamic>);
  }

  Future<void> setVolume(double volumeLvl, {bool isMuted = false}) async {
    final dto = SetVolumeRequestDto(volumeLevel: volumeLvl, isMuted: isMuted);
    await _invokeHubMethod('SetVolume', [dto]);
  }

  Future<void> updatePlayList(int id, String name) async {
    final dto = UpdatePlayListRequestDto(name: name);
    await _invokeHubMethod('UpdatePlayList', [id, dto]);
  }
}
