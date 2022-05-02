import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:collection/collection.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlists_bloc.freezed.dart';
part 'playlists_event.dart';
part 'playlists_state.dart';

class PlayListsBloc extends Bloc<PlayListsEvent, PlayListsState> {
  final CastItHubClientService _castItHub;

  PlayListsState get initialState => PlayListsState.loading();

  PlayListsBloc(this._castItHub) : super(PlayListsState.disconnected());

  _LoadedState get currentState => state as _LoadedState;

  @override
  Stream<PlayListsState> mapEventToState(PlayListsEvent event) async* {
    if (event is _Load) {
      yield initialState;
    }

    final s = event.map(
      load: (e) async {
        await _castItHub.loadPlayLists();
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
    _castItHub.connected.stream.listen((event) {
      add(PlayListsEvent.load());
    });

    _castItHub.disconnected.stream.listen((event) {
      add(PlayListsEvent.disconnected());
    });

    _castItHub.playListsChanged.stream.listen((event) {
      add(PlayListsEvent.loaded(playlists: event));
    });

    _castItHub.playListAdded.stream.listen((e) {
      state.maybeMap(
        loaded: (_) => add(PlayListsEvent.added(playList: e)),
        orElse: () {},
      );
    });

    _castItHub.playListChanged.stream.listen((e) {
      state.maybeMap(
        loaded: (state) {
          final changeComesFromPlayedFile = e.item1;
          final playList = e.item2;
          if (!changeComesFromPlayedFile) {
            add(PlayListsEvent.changed(playList: playList));
          }
        },
        orElse: () {},
      );
    });

    _castItHub.playListDeleted.stream.listen((e) {
      state.maybeMap(
        loaded: (_) => add(PlayListsEvent.deleted(id: e)),
        orElse: () {},
      );
    });
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
        if (current == null || current == playList) {
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
