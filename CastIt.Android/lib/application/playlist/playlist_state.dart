part of 'playlist_bloc.dart';

@freezed
class PlayListState with _$PlayListState {
  factory PlayListState.loading() = _LoadingState;

  factory PlayListState.loaded({
    required int playlistId,
    required String name,
    required int position,
    required bool loop,
    required bool shuffle,
    required List<FileItemResponseDto> files,
    required bool loaded,
    @Default([]) List<FileItemResponseDto> filteredFiles,
    @Default(false) bool searchBoxIsVisible,
    @Default(false) bool isFiltering,
    int? scrollToFileId,
  }) = _LoadedState;

  factory PlayListState.disconnected({int? playListId}) = _DisconnectedState;

  factory PlayListState.close() = _CloseState;

  factory PlayListState.notFound() = _NotFoundState;
}
