part of 'playlists_bloc.dart';

abstract class PlaylistsState extends Equatable {
  const PlaylistsState();
}

class PlayListsLoadingState extends PlaylistsState {
  @override
  List<Object> get props => [];
}

class PlayListsLoadedState extends PlaylistsState {
  final bool loaded;
  final List<PlayListResponseDto> playlists;
  final int reloads;

  @override
  List<Object> get props => [
        loaded,
        playlists,
        reloads,
      ];

  const PlayListsLoadedState({
    @required this.loaded,
    @required this.playlists,
    @required this.reloads,
  });

  PlayListsLoadedState copyWith({
    bool loaded,
    List<PlayListResponseDto> playlists,
    int reloads,
  }) {
    return PlayListsLoadedState(
      loaded: loaded ?? this.loaded,
      playlists: playlists ?? this.playlists,
      reloads: reloads ?? this.reloads,
    );
  }
}
