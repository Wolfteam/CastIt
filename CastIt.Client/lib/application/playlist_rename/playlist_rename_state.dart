part of 'playlist_rename_bloc.dart';

@freezed
sealed class PlayListRenameState with _$PlayListRenameState {
  const factory PlayListRenameState.loaded({required String currentName, required bool isNameValid}) =
      PlayListRenameStateLoadedState;
}
