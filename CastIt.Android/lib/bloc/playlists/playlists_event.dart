part of 'playlists_bloc.dart';

@freezed
abstract class PlayListsEvent implements _$PlayListsEvent {
  factory PlayListsEvent.load() = PlayListsLoadEvent;
  const PlayListsEvent._();
}
