part of 'played_file_item_bloc.dart';

@freezed
sealed class PlayedFileItemState with _$PlayedFileItemState {
  const factory PlayedFileItemState.notPlaying() = PlayedFileItemStateNotPlayingState;

  const factory PlayedFileItemState.playing({
    required int id,
    required int playListId,
    required double playedPercentage,
    required String playedTime,
    required String duration,
    required String fullTotalDuration,
  }) = PlayedFileItemStateLoadedState;
}
