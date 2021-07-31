part of 'played_play_list_item_bloc.dart';

@freezed
class PlayedPlayListItemState with _$PlayedPlayListItemState {
  const factory PlayedPlayListItemState.notPlaying() = _NotPlayingState;

  const factory PlayedPlayListItemState.playing({
    required int id,
    required String totalDuration,
  }) = _LoadedState;
}
