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

  const factory PlayListEvent.notFound() = PlayListNotFoundEvent;

  const factory PlayListEvent.playListChanged({
    required GetAllPlayListResponseDto playList,
  }) = _PlayListChanged;

  const factory PlayListEvent.playListDeleted({
    required int id,
  }) = _PlayListDeleted;

  const factory PlayListEvent.fileAdded({
    required FileItemResponseDto file,
  }) = _FileAdded;

  const factory PlayListEvent.fileChanged({
    required FileItemResponseDto file,
  }) = _FileChanged;

  const factory PlayListEvent.filesChanged({
    required List<FileItemResponseDto> files,
  }) = _FilesChanged;

  const factory PlayListEvent.fileDeleted({
    required int playListId,
    required int id,
  }) = _FilesDeleted;
}
