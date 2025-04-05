part of 'playlist_rename_bloc.dart';

@freezed
sealed class PlayListRenameEvent with _$PlayListRenameEvent {
  const factory PlayListRenameEvent.load({required String name}) = PlayListRenameEventLoad;

  const factory PlayListRenameEvent.nameChanged({required String name}) = PlayListRenameEventNameChanged;
}
