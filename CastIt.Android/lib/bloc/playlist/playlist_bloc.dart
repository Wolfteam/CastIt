import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../models/dtos/responses/file_item_response_dto.dart';
import '../../models/dtos/responses/playlist_item_response_dto.dart';
import '../server_ws/server_ws_bloc.dart';

part 'playlist_bloc.freezed.dart';
part 'playlist_event.dart';
part 'playlist_state.dart';

class PlayListBloc extends Bloc<PlayListEvent, PlayListState> {
  final ServerWsBloc _serverWsBloc;

  @override
  PlayListState get initialState => PlayListState.loading();

  PlayListBloc(this._serverWsBloc) {
    _serverWsBloc.playlistLoaded.stream.listen((event) {
      add(PlayListEvent.loaded(playlist: event));
    });
  }

  @override
  Stream<PlayListState> mapEventToState(
    PlayListEvent event,
  ) async* {
    yield initialState;
    final s = event.map(
      load: (e) async {
        await _serverWsBloc.loadPlayList(e.id);
        return initialState;
      },
      loaded: (e) async => PlayListState.loaded(
        playlistId: e.playlist.id,
        name: e.playlist.name,
        loop: e.playlist.loop,
        position: e.playlist.position,
        shuffle: e.playlist.shuffle,
        files: e.playlist.files,
        loaded: true,
      ),
    );

    yield await s;
  }
}
