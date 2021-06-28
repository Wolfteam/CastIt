part of 'play_bloc.dart';

@freezed
class PlayState with _$PlayState {
  factory PlayState.connecting() = _ConnectingState;

  factory PlayState.connected() = _ConnectedState;

  factory PlayState.fileLoading() = _FileLoadingState;

  factory PlayState.fileLoadingFailed({
    required String msg,
  }) = _FileLoadingFailedState;

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
  }) = _PlayingState;
}
