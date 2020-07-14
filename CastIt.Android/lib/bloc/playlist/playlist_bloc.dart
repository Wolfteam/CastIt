import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../common/extensions/string_extensions.dart';
import '../../models/dtos/responses/file_item_response_dto.dart';
import '../../models/dtos/responses/playlist_item_response_dto.dart';
import '../server_ws/server_ws_bloc.dart';

part 'playlist_bloc.freezed.dart';
part 'playlist_event.dart';
part 'playlist_state.dart';

class PlayListBloc extends Bloc<PlayListEvent, PlayListState> {
  final ServerWsBloc _serverWsBloc;

  PlayListState get initialState => PlayListState.loading();

  PlayListLoadedState get currentState => state as PlayListLoadedState;

  PlayListBloc(this._serverWsBloc) : super(PlayListState.loading()) {
    _serverWsBloc.playlistLoaded.stream.listen((event) {
      //Playlist does not exist
      if (event == null) {
        add(PlayListEvent.notFound());
        return;
      }

      if (state is! PlayListLoadedState || currentState.playlistId == event.id) {
        add(PlayListEvent.loaded(playlist: event));
        return;
      }
    });

    _serverWsBloc.disconnected.stream.listen((event) {
      add(const PlayListEvent.disconnected());
    });

    _serverWsBloc.refreshPlayList.stream.listen((event) {
      if (state is! PlayListLoadedState || currentState.playlistId != event.id) {
        return;
      }
      if (!event.wasDeleted) {
        add(PlayListEvent.load(id: event.id));
      } else {
        add(const PlayListEvent.closePage());
      }
    });
  }

  @override
  Stream<PlayListState> mapEventToState(
    PlayListEvent event,
  ) async* {
    if (event is PlayListLoadEvent || event is PlayListLoadedEvent) {
      //If there were playlists loaded, hide them all!
      yield initialState;
    }

    final s = event.map(
      disconnected: (e) async => PlayListState.disconnected(),
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
      playListOptionsChanged: (e) async => currentState.copyWith(loop: e.loop, shuffle: e.shuffle),
      toggleSearchBoxVisibility: (e) async => currentState.copyWith(
        searchBoxIsVisible: !currentState.searchBoxIsVisible,
      ),
      searchBoxTextChanged: (e) async {
        final isFiltering = !e.text.isNullEmptyOrWhitespace;
        final filteredFiles = !isFiltering
            ? <FileItemResponseDto>[]
            : currentState.files
                .where((element) => element.filename.toLowerCase().contains(e.text.toLowerCase()))
                .toList();

        return currentState.copyWith(filteredFiles: filteredFiles, isFiltering: isFiltering);
      },
      closePage: (e) async => PlayListState.close(),
      notFound: (_) async => PlayListState.notFound(),
    );

    yield await s;
  }
}
