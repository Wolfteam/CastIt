part of 'played_play_list_item_bloc.dart';

@freezed
class PlayedPlayListItemEvent with _$PlayedPlayListItemEvent {
  factory PlayedPlayListItemEvent.playing({
    required int id,
    required String totalDuration,
  }) = _Playing;

  factory PlayedPlayListItemEvent.endReached() = _EndReached;
}
