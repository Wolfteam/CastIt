import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../models/dtos/responses/get_all_playlist_response_dto.dart';
import '../server_ws/server_ws_bloc.dart';

part 'playlists_bloc.freezed.dart';
part 'playlists_event.dart';
part 'playlists_state.dart';

class PlayListsBloc extends Bloc<PlayListsEvent, PlayListsState> {
  final ServerWsBloc _serverWsBloc;

  PlayListsBloc(this._serverWsBloc) {
    _serverWsBloc.connected.stream.listen((event) {
      add(PlayListsEvent.load());
    });

    _serverWsBloc.disconnected.stream.listen((event) {
      add(PlayListsEvent.disconnected());
    });

    _serverWsBloc.playlistsLoaded.stream.listen((event) {
      add(PlayListsEvent.loaded(playlists: event));
    });
  }

  @override
  PlayListsState get initialState => PlayListsState.loading();

  PlayListsLoadedState get currentState => state as PlayListsLoadedState;

  @override
  Stream<PlayListsState> mapEventToState(
    PlayListsEvent event,
  ) async* {
    yield initialState;
    final s = event.map(
      load: (e) async {
        await _serverWsBloc.loadPlayLists();
        return initialState;
      },
      loaded: (e) async => PlayListsState.loaded(
        reloads: state is PlayListsLoadedState ? currentState.reloads + 1 : 1,
        playlists: e.playlists,
      ),
      disconnected: (e) async => PlayListsState.disconnected(),
    );

    yield await s;
  }
}
