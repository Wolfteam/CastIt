part of 'playlist_bloc.dart';

abstract class PlaylistState extends Equatable {
  const PlaylistState();
}

class PlaylistLoadingState extends PlaylistState {
  @override
  List<Object> get props => [];
}

class PlayListLoadedState extends PlaylistState {
  final int playlistId;
  final String name;
  final int position;
  final bool loop;
  final bool shuffle;
  final List<FileResponseDto> files;
  final bool loaded;

  int get numberOfFiles => files.length;

  @override
  List<Object> get props => [
        playlistId,
        name,
        position,
        loop,
        shuffle,
        files,
        loaded,
      ];

  const PlayListLoadedState({
    @required this.playlistId,
    @required this.name,
    @required this.position,
    @required this.loop,
    @required this.shuffle,
    @required this.files,
    @required this.loaded,
  });
}
