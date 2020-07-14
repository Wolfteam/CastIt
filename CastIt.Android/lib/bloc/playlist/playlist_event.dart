part of 'playlist_bloc.dart';

@freezed
abstract class PlayListEvent implements _$PlayListEvent {
  const factory PlayListEvent.loaded({
    @required PlayListItemResponseDto playlist,
  }) = PlayListLoadedEvent;

  const factory PlayListEvent.load({
    @required int id,
  }) = PlayListLoadEvent;

  const factory PlayListEvent.playListOptionsChanged({
    @required bool loop,
    bool shuffle,
  }) = PlayListOptionsChangedEvent;

  const factory PlayListEvent.disconnected() = PlayListDisconnectedEvent;

  const factory PlayListEvent.toggleSearchBoxVisibility() = PlayListToggleSearchBoxVisibilityEvent;

  const factory PlayListEvent.searchBoxTextChanged({
    @required String text,
  }) = PlayListSearchBoxTextChangedEvent;

  const factory PlayListEvent.closePage() = PlayListClosePageEvent;

  factory PlayListEvent.notFound() = PlayListNotFoundEvent;
}
