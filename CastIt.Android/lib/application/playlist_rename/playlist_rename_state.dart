part of 'playlist_rename_bloc.dart';

@freezed
class PlayListRenameState with _$PlayListRenameState {
  factory PlayListRenameState.initial() = _InitialState;

  factory PlayListRenameState.loaded({
    required String currentName,
    required bool isNameValid,
  }) = _LoadedState;
}
