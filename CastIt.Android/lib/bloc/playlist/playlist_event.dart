part of 'playlist_bloc.dart';

abstract class PlaylistEvent extends Equatable {
  const PlaylistEvent();
}

class LoadPlayList extends PlaylistEvent {
  final PlayListResponseDto playlist;

  @override
  List<Object> get props => [playlist];

  const LoadPlayList({
    @required this.playlist,
  });
}
