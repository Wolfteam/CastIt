part of 'playlists_bloc.dart';

@freezed
sealed class PlayListsState with _$PlayListsState {
  const factory PlayListsState.loading() = PlayListsStateLoadingState;

  const factory PlayListsState.loaded({required List<GetAllPlayListResponseDto> playlists, required int reloads}) =
      PlayListsStateLoadedState;

  const factory PlayListsState.disconnected() = PlayListsStateDisconnectedState;
}
