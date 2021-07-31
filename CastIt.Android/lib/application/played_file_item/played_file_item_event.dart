part of 'played_file_item_bloc.dart';

@freezed
class PlayedFileItemEvent with _$PlayedFileItemEvent {
  factory PlayedFileItemEvent.playing({
    required int id,
    required int playListId,
    required double playedPercentage,
    required String fullTotalDuration,
  }) = _Playing;

  factory PlayedFileItemEvent.endReached() = _EndReached;
}
