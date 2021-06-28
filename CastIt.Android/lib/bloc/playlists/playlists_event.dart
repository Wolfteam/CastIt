part of 'playlists_bloc.dart';

@freezed
class PlayListsEvent with _$PlayListsEvent {
  factory PlayListsEvent.load() = PlayListsLoadEvent;

  factory PlayListsEvent.loaded({
    required List<GetAllPlayListResponseDto> playlists,
  }) = PlayListsLoadedEvent;

  factory PlayListsEvent.added({
    required GetAllPlayListResponseDto playList,
  }) = _PlayListAdded;

  factory PlayListsEvent.changed({
    required GetAllPlayListResponseDto playList,
  }) = _PlayListChanged;

  factory PlayListsEvent.deleted({
    required int id,
  }) = _PlayListDeleted;

  factory PlayListsEvent.disconnected() = PlayListsDisconnectedEvent;

  const PlayListsEvent._();
}
