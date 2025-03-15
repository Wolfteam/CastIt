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

  PlayListsState get initialState => const PlayListsState.loading();

  PlayListsStateLoadedState get currentState => state as PlayListsStateLoadedState;

  PlayListsBloc(this._castItHub) : super(const PlayListsState.disconnected()) {
    on<PlayListsEventLoad>((event, emit) async {
      emit(initialState);
      await _castItHub.loadPlayLists();
      emit(initialState);
    });

    on<PlayListsEventLoaded>((event, emit) {
      final updatedState = PlayListsState.loaded(
        reloads: state is PlayListsStateLoadedState ? currentState.reloads + 1 : 1,
        playlists: event.playlists,
      );
      emit(updatedState);
    });

    on<PlayListsEventAdded>((event, emit) => emit(_handlePlayListAdded(event.playList)));

    on<PlayListsEventChanged>((event, emit) => emit(_handlePlayListChanged(event.playList)));

    on<PlayListsEventDeleted>((event, emit) => emit(_handlePlayListDeleted(event.id)));

    on<PlayListsEventDisconnected>((event, emit) => emit(const PlayListsState.disconnected()));
  }

  void listenHubEvents() {
    _castItHub.connected.stream.listen((event) {
      add(const PlayListsEvent.load());
    });

    _castItHub.disconnected.stream.listen((event) {
      add(const PlayListsEvent.disconnected());
    });

    _castItHub.playListsChanged.stream.listen((event) {
      add(PlayListsEvent.loaded(playlists: event));
    });

    _castItHub.playListAdded.stream.listen((e) {
      switch (state) {
        case PlayListsStateLoadedState():
          add(PlayListsEvent.added(playList: e));
        default:
          break;
      }
    });

    _castItHub.playListChanged.stream.listen((e) {
      switch (state) {
        case PlayListsStateLoadedState():
          final changeComesFromPlayedFile = e.$1;
          final playList = e.$2;
          if (!changeComesFromPlayedFile) {
            add(PlayListsEvent.changed(playList: playList));
          }
        default:
          break;
      }
    });

    _castItHub.playListDeleted.stream.listen((e) {
      switch (state) {
        case PlayListsStateLoadedState():
          add(PlayListsEvent.deleted(id: e));
        default:
          break;
      }
    });
  }

  PlayListsState _handlePlayListAdded(GetAllPlayListResponseDto playList) {
    switch (state) {
      case final PlayListsStateLoadedState state:
        final playLists = [...state.playlists];
        playLists.insert(playList.position - 1, playList);
        return state.copyWith(playlists: playLists);
      default:
        return state;
    }
  }

  PlayListsState _handlePlayListChanged(GetAllPlayListResponseDto playList) {
    switch (state) {
      case final PlayListsStateLoadedState state:
        final current = state.playlists.firstWhereOrNull((el) => el.id == playList.id);
        if (current == null || current == playList) {
          return state;
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

        final currentIndex = state.playlists.indexOf(current);
        final playLists = [...state.playlists];
        playLists.removeAt(currentIndex);
        playLists.insert(currentIndex, updated);

        return state.copyWith(playlists: playLists);
      default:
        return state;
    }
  }

  PlayListsState _handlePlayListDeleted(int id) {
    switch (state) {
      case final PlayListsStateLoadedState state:
        final playLists = [...state.playlists];
        playLists.removeWhere((el) => el.id == id);
        return state.copyWith(playlists: playLists);
      default:
        return state;
    }
  }
}
