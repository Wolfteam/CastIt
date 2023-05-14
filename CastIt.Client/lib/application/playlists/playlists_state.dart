part of 'playlists_bloc.dart';

@freezed
class PlayListsState with _$PlayListsState {
  factory PlayListsState.loading() = _LoadingState;

  factory PlayListsState.loaded({
    required List<GetAllPlayListResponseDto> playlists,
    required int reloads,
  }) = _LoadedState;

  factory PlayListsState.disconnected() = _DisconnectedState;
}
