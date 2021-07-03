import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'server_ws_bloc.freezed.dart';
part 'server_ws_event.dart';
part 'server_ws_state.dart';

class ServerWsBloc extends Bloc<ServerWsEvent, ServerWsState> {
  final SettingsService _settings;
  final LoggingService _logger;
  final CastItHubClientService _castItHub;

  ServerWsState get initialState => ServerWsState.loading();

  _LoadedState get currentState => state as _LoadedState;

  ServerWsBloc(this._logger, this._settings, this._castItHub) : super(ServerWsState.loading()) {
    _castItHub.serverMessageReceived.stream.listen((e) => add(ServerWsEvent.showMsg(type: e)));
  }

  @override
  Stream<ServerWsState> mapEventToState(ServerWsEvent event) async* {
    if (event is _Disconnected && _castItHub.isConnected) {
      _logger.info(runtimeType, 'A server disconnected from ws event was raised but the server is running');
      return;
    }
    _logger.info(runtimeType, 'Handling event = $event');
    final s = event.when(
      connectToWs: () async {
        if (!_castItHub.isConnected) {
          await _castItHub.connectToHub();
        }
        return ServerWsState.loaded(
          castItUrl: _settings.castItUrl,
          connectionRetries: 0,
          isConnectedToWs: _castItHub.isConnected,
        );
      },
      disconnectedFromWs: () async {
        await _castItHub.disconnectFromHub(triggerEvent: false);
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries! + 1,
        );
      },
      disconnectFromWs: () async {
        await _castItHub.disconnectFromHub();
        return currentState.copyWith(
          isConnectedToWs: false,
          castItUrl: _settings.castItUrl,
          connectionRetries: currentState.connectionRetries! + 1,
        );
      },
      updateUrlAndConnectToWs: (newUrl) async {
        _settings.castItUrl = newUrl.trim();
        await _castItHub.connectToHub();
        return currentState.copyWith(
          isConnectedToWs: _castItHub.isConnected,
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
    await _castItHub.dispose();
    await super.close();
  }

  Future<void> playFile(int id, int playListId, {bool force = false}) async {
    await _castItHub.playFile(id, playListId, force: force);
  }

  Future<void> gotoSeconds(double seconds) async {
    await _castItHub.gotoSeconds(seconds);
  }

  Future<void> skipSeconds(double seconds) async {
    await _castItHub.skipSeconds(seconds);
  }

  Future<void> goTo({bool next = false, bool previous = false}) async {
    await _castItHub.goTo(next: next, previous: previous);
  }

  Future<void> togglePlayBack() async {
    await _castItHub.togglePlayBack();
  }

  Future<void> stopPlayBack() async {
    await _castItHub.stopPlayBack();
  }

  Future<void> setPlayListOptions(int id, {bool loop = false, bool shuffle = false}) async {
    await _castItHub.setPlayListOptions(id, loop: loop, shuffle: shuffle);
  }

  Future<void> deletePlayList(int id) async {
    await _castItHub.deletePlayList(id);
  }

  Future<void> deleteFile(int id, int playListId) async {
    await _castItHub.deleteFile(id, playListId);
  }

  Future<void> loopFile(int id, int playListId, {bool loop = false}) async {
    await _castItHub.loopFile(id, playListId, loop: loop);
  }

  Future<void> setFileOptions(int streamIndex, {bool isAudio = false, bool isSubtitle = false, bool isQuality = false}) async {
    await _castItHub.setFileOptions(streamIndex, isAudio: isAudio, isSubtitle: isSubtitle, isQuality: isQuality);
  }

  Future<void> updateSettings(ServerAppSettings dto) async {
    await _castItHub.updateSettings(dto);
  }

  Future<void> loadPlayLists() async {
    await _castItHub.loadPlayLists();
  }

  Future<PlayListItemResponseDto> loadPlayList(int playListId) {
    return _castItHub.loadPlayList(playListId);
  }

  Future<void> setVolume(double volumeLvl, {bool isMuted = false}) async {
    await _castItHub.setVolume(volumeLvl, isMuted: isMuted);
  }

  Future<void> updatePlayList(int id, String name) async {
    await _castItHub.updatePlayList(id, name);
  }
}
