part of 'playlist_bloc.dart';

@freezed
abstract class PlayListEvent implements _$PlayListEvent {
  const factory PlayListEvent.load({
    @required PlayListResponseDto playList,
  }) = PlayListLoadEvent;
}
