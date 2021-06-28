import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../server_ws/server_ws_bloc.dart';

part 'playlists_bloc.freezed.dart';
part 'playlists_event.dart';
part 'playlists_state.dart';

class PlayListsBloc extends Bloc<PlayListsEvent, PlayListsState> {
  final ServerWsBloc _serverWsBloc;

  PlayListsState get initialState => PlayListsState.loading();

  PlayListsBloc(this._serverWsBloc) : super(PlayListsState.loaded(playlists: [], reloads: 0)) {
    // _serverWsBloc.connected.stream.listen((event) {
    //   add(PlayListsEvent.load());
    // });
  }

  _LoadedState get currentState => state as _LoadedState;

  @override
  Stream<PlayListsState> mapEventToState(PlayListsEvent event) async* {
    if (event is _Load) {
      yield initialState;
    }

    final s = event.map(
      load: (e) async {
        await _serverWsBloc.loadPlayLists();
        return initialState;
      },
      loaded: (e) async => PlayListsState.loaded(
        reloads: state is _LoadedState ? currentState.reloads + 1 : 1,
        playlists: e.playlists,
      ),
      added: (e) async => _handlePlayListAdded(e.playList),
      changed: (e) async => _handlePlayListChanged(e.playList),
      deleted: (e) async => _handlePlayListDeleted(e.id),
      disconnected: (e) async => PlayListsState.disconnected(),
    );

    yield await s;
  }

  void listenHubEvents() {
    _serverWsBloc.disconnected.stream.listen((event) => add(PlayListsEvent.disconnected()));

    _serverWsBloc.playListsChanged.stream.listen((event) => add(PlayListsEvent.loaded(playlists: event)));

    _serverWsBloc.playListAdded.stream.listen((e) => add(PlayListsEvent.added(playList: e)));

    _serverWsBloc.playListChanged.stream.listen((e) => add(PlayListsEvent.changed(playList: e)));

    _serverWsBloc.playListDeleted.stream.listen((e) => add(PlayListsEvent.deleted(id: e)));
  }

  PlayListsState _handlePlayListAdded(GetAllPlayListResponseDto playList) {
    return state.map(
      loading: (s) => s,
      loaded: (s) {
        final playLists = [...s.playlists];
        playLists.insert(playList.position - 1, playList);

        return s.copyWith.call(playlists: playLists);
      },
      disconnected: (s) => s,
    );
  }

  PlayListsState _handlePlayListChanged(GetAllPlayListResponseDto playList) {
    return state.map(
      loading: (s) => s,
      loaded: (s) {
        final current = s.playlists.firstWhereOrNull((el) => el.id == playList.id);
        if (current == null) {
          return s;
        }
        final updated = current.copyWith.call(
          imageUrl: playList.imageUrl,
          loop: playList.loop,
          name: playList.name,
          numberOfFiles: playList.numberOfFiles,
          playedTime: playList.playedTime,
          position: playList.position,
          shuffle: playList.shuffle,
          totalDuration: playList.totalDuration,
        );

        final currentIndex = s.playlists.indexOf(current);
        final playLists = [...s.playlists];
        playLists.removeAt(currentIndex);
        playLists.insert(currentIndex, updated);

        return s.copyWith.call(playlists: playLists);
      },
      disconnected: (s) => s,
    );
  }

  PlayListsState _handlePlayListDeleted(int id) {
    return state.map(
      loading: (s) => s,
      loaded: (s) {
        final playLists = [...s.playlists];
        playLists.removeWhere((el) => el.id == id);
        return s.copyWith.call(playlists: playLists);
      },
      disconnected: (s) => s,
    );
  }
}
