part of 'play_bloc.dart';

@freezed
sealed class PlayState with _$PlayState {
  const factory PlayState.connecting() = PlayStateConnectingState;

  const factory PlayState.connected() = PlayStateConnectedState;

  const factory PlayState.fileLoading() = PlayStateFileLoadingState;

  const factory PlayState.fileLoadingFailed({required String msg}) = PlayStateFileLoadingFailedState;

  const factory PlayState.playing({
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
    String? playListPlayedTime,
    String? playListTotalDuration,
  }) = PlayStatePlayingState;
}
