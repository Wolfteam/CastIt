part of 'playlists_bloc.dart';

@freezed
class PlayListsEvent with _$PlayListsEvent {
  factory PlayListsEvent.load() = PlayListsLoadEvent;

  factory PlayListsEvent.loaded({
    required List<GetAllPlayListResponseDto> playlists,
  }) = PlayListsLoadedEvent;

  factory PlayListsEvent.disconnected() = PlayListsDisconnectedEvent;

  const PlayListsEvent._();
}
