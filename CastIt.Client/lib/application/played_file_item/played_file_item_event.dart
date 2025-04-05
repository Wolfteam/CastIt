part of 'played_file_item_bloc.dart';

@freezed
sealed class PlayedFileItemEvent with _$PlayedFileItemEvent {
  const factory PlayedFileItemEvent.playing({
    required int id,
    required int playListId,
    required double playedPercentage,
    required String fullTotalDuration,
  }) = PlayedFileItemEventPlaying;

  const factory PlayedFileItemEvent.endReached() = PlayedFileItemEventEndReached;
}
