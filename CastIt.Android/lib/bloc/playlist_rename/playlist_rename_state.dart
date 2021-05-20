part of 'playlist_rename_bloc.dart';

@freezed
class PlayListRenameState with _$PlayListRenameState {
  factory PlayListRenameState.initial() = PlayListRenameInitialState;

  factory PlayListRenameState.loaded({
    required String currentName,
    required bool isNameValid,
  }) = PlayListRenameLoadedState;

  const PlayListRenameState._();
}
