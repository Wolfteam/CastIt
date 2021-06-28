part of 'playlist_bloc.dart';

@freezed
class PlayListEvent with _$PlayListEvent {
  const factory PlayListEvent.loaded({
    required PlayListItemResponseDto playlist,
  }) = _Loaded;

  const factory PlayListEvent.load({
    required int id,
  }) = _Load;

  const factory PlayListEvent.playListOptionsChanged({
    required bool loop,
    required bool shuffle,
  }) = _PlayListOptionsChanged;

  const factory PlayListEvent.disconnected({int? playListId}) = _Disconnected;

  const factory PlayListEvent.toggleSearchBoxVisibility() = _ToggleSearchBoxVisibility;

  const factory PlayListEvent.searchBoxTextChanged({
    required String text,
  }) = _SearchBoxTextChanged;

  const factory PlayListEvent.closePage() = _ClosePage;

  const factory PlayListEvent.notFound() = _PlayListNotFound;

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
