part of 'playlists_bloc.dart';

abstract class PlaylistsEvent extends Equatable {
  const PlaylistsEvent();
}

class LoadPlayLists extends PlaylistsEvent {
  @override
  List<Object> get props => [];
}
