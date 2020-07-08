part of 'playlists_bloc.dart';

@freezed
abstract class PlayListsState implements _$PlayListsState {
  factory PlayListsState.loading() = PlayListsLoadingState;
  factory PlayListsState.loaded({
    @required List<GetAllPlayListResponseDto> playlists,
    @required int reloads,
  }) = PlayListsLoadedState;
  factory PlayListsState.disconnected() = PlayListsDisconnectedState;
  const PlayListsState._();
}
