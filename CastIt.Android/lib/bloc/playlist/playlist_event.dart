part of 'playlist_bloc.dart';

@freezed
abstract class PlayListEvent implements _$PlayListEvent {
  const factory PlayListEvent.loaded({
    @required PlayListItemResponseDto playlist,
  }) = PlayListLoadedEvent;

  const factory PlayListEvent.load({
    @required int id,
  }) = PlayListLoadEvent;
}
