part of 'playlist_bloc.dart';

@freezed
class PlayListState with _$PlayListState {
  factory PlayListState.loading() = PlayListLoadingState;

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
  }) = PlayListLoadedState;

  factory PlayListState.disconnected({int? playListId}) = PlayListDisconnectedState;

  factory PlayListState.close() = PlayListCloseState;

  factory PlayListState.notFound() = PlayListNotFoundState;

  const PlayListState._();
}
