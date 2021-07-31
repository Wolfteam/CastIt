part of 'played_file_item_bloc.dart';

@freezed
class PlayedFileItemState with _$PlayedFileItemState {
  const factory PlayedFileItemState.notPlaying() = _NotPlayingState;

  const factory PlayedFileItemState.playing({
    required int id,
    required int playListId,
    required double playedPercentage,
    required String fullTotalDuration,
  }) = _LoadedState;
}
