part of 'playlists_bloc.dart';

@freezed
abstract class PlayListsEvent implements _$PlayListsEvent {
  factory PlayListsEvent.load() = PlayListsLoadEvent;
  factory PlayListsEvent.loaded({
    @required List<GetAllPlayListResponseDto> playlists,
  }) = PlayListsLoadedEvent;
  factory PlayListsEvent.disconnected() = PlayListsDisconnectedEvent;
  const PlayListsEvent._();
}
