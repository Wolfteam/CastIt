part of 'played_play_list_item_bloc.dart';

@freezed
sealed class PlayedPlayListItemState with _$PlayedPlayListItemState {
  const factory PlayedPlayListItemState.notPlaying() = PlayedPlayListItemStateNotPlayingState;

  const factory PlayedPlayListItemState.playing({required int id, required String totalDuration}) =
      PlayedPlayListItemStateLoadedState;
}
