part of 'playlist_bloc.dart';

@freezed
sealed class PlayListEvent with _$PlayListEvent {
  const factory PlayListEvent.loaded({required PlayListItemResponseDto playlist}) = PlayListEventLoaded;

  const factory PlayListEvent.load({required int id, int? scrollToFileId}) = PlayListEventLoad;

  const factory PlayListEvent.playListOptionsChanged({required bool loop, required bool shuffle}) =
      PlayListEventPlayListOptionsChanged;

  const factory PlayListEvent.disconnected({int? playListId}) = PlayListEventDisconnected;

  const factory PlayListEvent.toggleSearchBoxVisibility() = PlayListEventToggleSearchBoxVisibility;

  const factory PlayListEvent.searchBoxTextChanged({required String text}) = PlayListEventSearchBoxTextChanged;

  const factory PlayListEvent.closePage() = PlayListEventClosePage;

  const factory PlayListEvent.notFound() = PlayListEventPlayListNotFound;

  const factory PlayListEvent.playListChanged({required GetAllPlayListResponseDto playList}) = PlayListEventPlayListChanged;

  const factory PlayListEvent.playListDeleted({required int id}) = PlayListEventPlayListDeleted;

  const factory PlayListEvent.fileAdded({required FileItemResponseDto file}) = PlayListEventFileAdded;

  const factory PlayListEvent.fileChanged({required FileItemResponseDto file}) = PlayListEventFileChanged;

  const factory PlayListEvent.filesChanged({required List<FileItemResponseDto> files}) = PlayListEventFilesChanged;

  const factory PlayListEvent.fileDeleted({required int playListId, required int id}) = PlayListEventFilesDeleted;
}
