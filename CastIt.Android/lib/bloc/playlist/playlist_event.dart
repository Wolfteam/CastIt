part of 'playlist_bloc.dart';

@freezed
class PlayListEvent with _$PlayListEvent {
  const factory PlayListEvent.loaded({
    required PlayListItemResponseDto playlist,
  }) = PlayListLoadedEvent;

  const factory PlayListEvent.load({
    required int id,
  }) = PlayListLoadEvent;

  const factory PlayListEvent.playListOptionsChanged({
    required bool loop,
    required bool shuffle,
  }) = PlayListOptionsChangedEvent;

  const factory PlayListEvent.disconnected({int? playListId}) = PlayListDisconnectedEvent;

  const factory PlayListEvent.toggleSearchBoxVisibility() = PlayListToggleSearchBoxVisibilityEvent;

  const factory PlayListEvent.searchBoxTextChanged({
    required String text,
  }) = PlayListSearchBoxTextChangedEvent;

  const factory PlayListEvent.closePage() = PlayListClosePageEvent;

  factory PlayListEvent.notFound() = PlayListNotFoundEvent;
}
