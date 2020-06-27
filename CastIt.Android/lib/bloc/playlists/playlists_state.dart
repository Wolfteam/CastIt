part of 'playlists_bloc.dart';

@freezed
abstract class PlayListsState implements _$PlayListsState {
  factory PlayListsState.loading() = PlayListsLoadingState;
  factory PlayListsState.loaded({
    @required bool loaded,
    @required List<PlayListResponseDto> playlists,
    @required int reloads,
  }) = PlayListsLoadedState;
  const PlayListsState._();
}
