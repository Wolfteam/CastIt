import 'dart:async';

import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:signalr_core/signalr_core.dart';
import 'package:tuple/tuple.dart';

const String _sendPlayLists = 'SendPlayLists';
const String _stoppedPlayback = 'StoppedPlayBack';
const String _playListAdded = 'PlayListAdded';
const String _playListChanged = 'PlayListChanged';
const String _playListsChanged = 'PlayListsChanged';
const String _playListDeleted = 'PlayListDeleted';
const String _playListBusy = 'PlayListIsBusy';
const String _fileAdded = 'FileAdded';
const String _fileChanged = 'FileChanged';
const String _filesChanged = 'FilesChanged';
const String _fileDeleted = 'FileDeleted';
const String _fileLoading = 'FileLoading';
const String _fileLoaded = 'FileLoaded';
const String _fileEndReached = 'FileEndReached';
const String _playerStatusChanged = 'PlayerStatusChanged';
const String _playerSettingsChanged = 'PlayerSettingsChanged';
const String _serverMessage = 'ServerMessage';
const String _castDeviceSet = 'CastDeviceSet';
const String _castDevicesChanged = 'CastDevicesChanged';
const String _castDeviceDisconnected = 'CastDeviceDisconnected';

class CastItHubClientServiceImpl implements CastItHubClientService {
  @override
  final StreamController<void> connected = StreamController.broadcast();

  @override
  final StreamController<void> fileLoading = StreamController.broadcast();

  @override
  final StreamController<PlayedFile> fileLoaded = StreamController.broadcast();

  @override
  final StreamController<String> fileLoadingError = StreamController.broadcast();

  @override
  final StreamController<double> fileTimeChanged = StreamController.broadcast();

  @override
  final StreamController<void> filePaused = StreamController.broadcast();

  @override
  final StreamController<void> fileEndReached = StreamController.broadcast();

  @override
  final StreamController<void> disconnected = StreamController.broadcast();

  @override
  final StreamController<void> appClosing = StreamController.broadcast();

  @override
  final StreamController<ServerAppSettings> settingsChanged = StreamController.broadcast();

  @override
  final StreamController<PlayListItemResponseDto?> playlistLoaded = StreamController.broadcast();

  @override
  final StreamController<List<FileItemOptionsResponseDto>> fileOptionsLoaded = StreamController.broadcast();

  @override
  final StreamController<VolumeLevelChangedResponseDto> volumeLevelChanged = StreamController.broadcast();

  @override
  final StreamController<RefreshPlayListResponseDto> refreshPlayList = StreamController.broadcast();

  @override
  final StreamController<AppMessageType> serverMessageReceived = StreamController.broadcast();

  @override
  final StreamController<GetAllPlayListResponseDto> playListAdded = StreamController.broadcast();

  @override
  final StreamController<List<GetAllPlayListResponseDto>> playListsChanged = StreamController.broadcast();

  @override
  final StreamController<Tuple2<bool, GetAllPlayListResponseDto>> playListChanged = StreamController.broadcast();

  @override
  final StreamController<int> playListDeleted = StreamController.broadcast();

  @override
  final StreamController<FileItemResponseDto> fileAdded = StreamController.broadcast();

  @override
  final StreamController<Tuple2<bool, FileItemResponseDto>> fileChanged = StreamController.broadcast();

  @override
  final StreamController<List<FileItemResponseDto>> filesChanged = StreamController.broadcast();

  @override
  final StreamController<Tuple2<int, int>> fileDeleted = StreamController.broadcast();

  final LoggingService _logger;
  final SettingsService _settings;
  HubConnection? _connection;

  @override
  bool get isConnected => _connection?.state == HubConnectionState.connected;

  CastItHubClientServiceImpl(this._logger, this._settings);

  @override
  Future<void> connectToHub() async {
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
          }
        },
      );

      await disconnectFromHub(triggerEvent: false);

      _connection = HubConnectionBuilder().withUrl(url, options).build();
      _listenHubEvents(_connection!);
      await _connection!.start();
    } catch (e, s) {
      _logger.error(runtimeType, '_connectToHub: Error while trying to connect to hub = $url', e, s);
      await disconnectFromHub();
    }
  }

  @override
  Future<void> dispose() async {
    await disconnectFromHub();
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
      serverMessageReceived.close(),
    ]);
  }

  @override
  Future<void> disconnectFromHub({bool triggerEvent = true}) async {
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
  }

  void _listenHubEvents(HubConnection connection) {
    connection.onclose((exception) async {
      await _onHubErrorOrDone(exception);
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
      case _playerSettingsChanged:
        _logger.info(runtimeType, '_handleSocketMsg: Settings were loaded');
        final settings = ServerAppSettings.fromJson(message as Map<String, dynamic>);
        connected.add(null);
        settingsChanged.add(settings);
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
      case _playListAdded:
        final newPl = GetAllPlayListResponseDto.fromJson(message as Map<String, dynamic>);
        playListAdded.add(newPl);
      case _playListChanged:
        final updatedPl = GetAllPlayListResponseDto.fromJson(message as Map<String, dynamic>);
        playListChanged.add(Tuple2(false, updatedPl));
      case _playListsChanged:
        final playLists = (message as List<dynamic>).map((e) => GetAllPlayListResponseDto.fromJson(e as Map<String, dynamic>)).toList();
        playListsChanged.add(playLists);
      case _playListDeleted:
        final id = message as int;
        playListDeleted.add(id);
      case _playListBusy:
        break;
      case _fileAdded:
        final file = FileItemResponseDto.fromJson(message as Map<String, dynamic>);
        fileAdded.add(file);
      case _fileChanged:
        final file = FileItemResponseDto.fromJson(message as Map<String, dynamic>);
        fileChanged.add(Tuple2(false, file));
      case _filesChanged:
        final files = (message as List<dynamic>).map((e) => FileItemResponseDto.fromJson(e as Map<String, dynamic>)).toList();
        filesChanged.add(files);
      case _fileDeleted:
        final items = message as List<dynamic>;
        final playListId = items.first as int;
        final fileId = items.last as int;
        fileDeleted.add(Tuple2(playListId, fileId));
      case _fileLoading:
        _logger.info(runtimeType, '_handleSocketMsg: A file is loading...');
        fileLoading.add(null);
      case _serverMessage:
        final code = message as int;
        serverMessageReceived.add(getAppMessageType(code));
      case _stoppedPlayback:
      case _fileEndReached:
        _logger.info(runtimeType, '_handleSocketMsg: File end reached');
        fileEndReached.add(null);
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

  Future<void> _onHubErrorOrDone(Exception? exception) async {
    if (exception != null) {
      _logger.error(runtimeType, '_listenHubEvents: Channel error method was called', exception);
    } else {
      _logger.info(runtimeType, '_listenHubEvents: Channel done method was called');
    }
    if (!isConnected) {
      await disconnectFromHub();
    }
  }

  Future<dynamic> _invokeHubMethod(String methodName, List<dynamic> args) async {
    try {
      _logger.info(runtimeType, '_invokeHubMethod: Trying to call method  = $methodName');
      if (_connection == null) {
        _logger.info(runtimeType, '_invokeHubMethod: Connection is null, trying to establish connection before calling method = $methodName');
        await connectToHub();
      }

      if (!isConnected) {
        _logger.warning(runtimeType, '_invokeHubMethod: Cannot invoke method  = $methodName cause the server is not running');
        await disconnectFromHub();
        return;
      }

      return await _connection!.invoke(methodName, args: args);
    } catch (e, s) {
      await disconnectFromHub();
      _logger.error(runtimeType, '_invokeHubMethod: Unknown error', e, s);
    }
    return null;
  }

  @override
  Future<void> playFile(int id, int playListId, {bool force = false}) async {
    final dto = PlayFileRequestDto(id: id, playListId: playListId, force: force);
    await _invokeHubMethod('Play', [dto]);
  }

  @override
  Future<void> gotoSeconds(double seconds) async {
    await _invokeHubMethod('GoToSeconds', [seconds]);
  }

  @override
  Future<void> skipSeconds(double seconds) async {
    await _invokeHubMethod('SkipSeconds', [seconds]);
  }

  @override
  Future<void> goTo({bool next = false, bool previous = false}) async {
    await _invokeHubMethod('GoTo', [next, previous]);
  }

  @override
  Future<void> togglePlayBack() async {
    await _invokeHubMethod('TogglePlayBack', []);
  }

  @override
  Future<void> stopPlayBack() async {
    await _invokeHubMethod('StopPlayback', []);
  }

  @override
  Future<void> setPlayListOptions(int id, {bool loop = false, bool shuffle = false}) async {
    final dto = SetPlayListOptionsRequestDto(loop: loop, shuffle: shuffle);
    await _invokeHubMethod('SetPlayListOptions', [id, dto]);
  }

  @override
  Future<void> deletePlayList(int id) async {
    await _invokeHubMethod('DeletePlayList', [id]);
  }

  @override
  Future<void> deleteFile(int id, int playListId) async {
    await _invokeHubMethod('DeleteFile', [playListId, id]);
  }

  @override
  Future<void> loopFile(int id, int playListId, {bool loop = false}) async {
    await _invokeHubMethod('LoopFile', [playListId, id, loop]);
  }

  @override
  Future<void> setFileOptions(int streamIndex, {bool isAudio = false, bool isSubtitle = false, bool isQuality = false}) async {
    final dto = SetFileOptionsRequestDto(streamIndex: streamIndex, isAudio: isAudio, isQuality: isQuality, isSubTitle: isSubtitle);
    await _invokeHubMethod('SetFileOptions', [dto]);
  }

  @override
  Future<void> updateSettings(ServerAppSettings dto) async {
    await _invokeHubMethod('UpdateSettings', [dto]);
  }

  @override
  Future<void> loadPlayLists() async {
    await _invokeHubMethod('SendPlayListsToClient', []);
  }

  @override
  Future<PlayListItemResponseDto> loadPlayList(int playListId) async {
    final response = await _invokeHubMethod('GetPlayList', [playListId]);
    return PlayListItemResponseDto.fromJson(response as Map<String, dynamic>);
  }

  @override
  Future<void> setVolume(double volumeLvl, {bool isMuted = false}) async {
    final dto = SetVolumeRequestDto(volumeLevel: volumeLvl, isMuted: isMuted);
    await _invokeHubMethod('SetVolume', [dto]);
  }

  @override
  Future<void> updatePlayList(int id, String name) async {
    final dto = UpdatePlayListRequestDto(name: name);
    await _invokeHubMethod('UpdatePlayList', [id, dto]);
  }
}
