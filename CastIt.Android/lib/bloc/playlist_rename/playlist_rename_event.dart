part of 'playlist_rename_bloc.dart';

@freezed
abstract class PlayListRenameEvent implements _$PlayListRenameEvent {
  factory PlayListRenameEvent.load({
    @required String name,
  }) = PlayListLoadEvent;

  factory PlayListRenameEvent.nameChanged({
    @required String name,
  }) = PlayListNameChangedEvent;

  const PlayListRenameEvent._();
}
