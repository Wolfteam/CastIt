part of 'played_play_list_item_bloc.dart';

@freezed
sealed class PlayedPlayListItemEvent with _$PlayedPlayListItemEvent {
  const factory PlayedPlayListItemEvent.playing({required int id, required String totalDuration}) =
      PlayedPlayListItemEventPlaying;

  const factory PlayedPlayListItemEvent.endReached() = PlayedPlayListItemEventEndReached;
}
