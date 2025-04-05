part of 'playlist_bloc.dart';

@freezed
sealed class PlayListState with _$PlayListState {
  const factory PlayListState.loading() = PlayListStateLoadingState;

  const factory PlayListState.loaded({
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
  }) = PlayListStateLoadedState;

  const factory PlayListState.disconnected({int? playListId}) = PlayListStateDisconnectedState;

  const factory PlayListState.close() = PlayListStateCloseState;

  const factory PlayListState.notFound() = PlayListStateNotFoundState;
}
