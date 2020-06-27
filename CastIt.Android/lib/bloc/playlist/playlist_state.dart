part of 'playlist_bloc.dart';

@freezed
abstract class PlayListState implements _$PlayListState {
  factory PlayListState.loading() = PlayListLoadingState;
  factory PlayListState.loaded({
    @required int playlistId,
    @required String name,
    @required int position,
    @required bool loop,
    @required bool shuffle,
    @required List<FileResponseDto> files,
    @required bool loaded,
  }) = PlayListLoadedState;

  const PlayListState._();
}
