part of 'playlists_bloc.dart';

@freezed
class PlayListsState with _$PlayListsState {
  factory PlayListsState.loading() = PlayListsLoadingState;

  factory PlayListsState.loaded({
    required List<GetAllPlayListResponseDto> playlists,
    required int reloads,
  }) = PlayListsLoadedState;

  factory PlayListsState.disconnected() = PlayListsDisconnectedState;

  const PlayListsState._();
}
