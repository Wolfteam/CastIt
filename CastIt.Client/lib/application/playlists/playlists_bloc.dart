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

  _LoadedState get currentState => state as _LoadedState;

  PlayListsBloc(this._castItHub) : super(PlayListsState.disconnected()) {
    on<_Load>((event, emit) async {
      emit(initialState);
      await _castItHub.loadPlayLists();
      emit(initialState);
    });

    on<_Loaded>((event, emit) {
      final updatedState = PlayListsState.loaded(
        reloads: state is _LoadedState ? currentState.reloads + 1 : 1,
        playlists: event.playlists,
      );
      emit(updatedState);
    });

    on<_Added>((event, emit) => emit(_handlePlayListAdded(event.playList)));

    on<_Changed>((event, emit) => emit(_handlePlayListChanged(event.playList)));

    on<_Deleted>((event, emit) => emit(_handlePlayListDeleted(event.id)));

    on<_Disconnected>((event, emit) => emit(PlayListsState.disconnected()));
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
          final changeComesFromPlayedFile = e.$1;
          final playList = e.$2;
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
