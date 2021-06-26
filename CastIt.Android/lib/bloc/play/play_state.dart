part of 'play_bloc.dart';

@freezed
class PlayState with _$PlayState {
  factory PlayState.connecting() = ConnectingState;
  factory PlayState.connected() = ConnectedState;
  factory PlayState.fileLoading() = FileLoadingState;
  factory PlayState.fileLoadingFailed({
    required String msg,
  }) = FileLoadingFailedState;
  factory PlayState.playing({
    int? id,
    int? playListId,
    String? filename,
    String? playlistName,
    required bool loopFile,
    required bool loopPlayList,
    required bool shufflePlayList,
    double? duration,
    double? currentSeconds,
    bool? isPaused,
    String? thumbPath,
    bool? isDraggingSlider,
  }) = PlayingState;
}
