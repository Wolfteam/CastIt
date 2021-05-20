part of 'play_bloc.dart';

@freezed
class PlayEvent with _$PlayEvent {
  factory PlayEvent.connected() = Connected;

  factory PlayEvent.fileLoading() = FileLoading;

  factory PlayEvent.fileLoadingError({required String msg}) = FileLoadingError;

  factory PlayEvent.fileLoaded({
    required int id,
    required String filename,
    String? thumbPath,
    required double duration,
    required bool loopFile,
    required double currentSeconds,
    required bool isPaused,
    required double volumeLevel,
    required bool isMuted,
    required int playListId,
    required String playlistName,
    required bool loopPlayList,
    required bool shufflePlayList,
  }) = Playing;

  factory PlayEvent.timeChanged({
    required double seconds,
  }) = TimeChanged;

  factory PlayEvent.paused() = Paused;

  factory PlayEvent.stopped() = Stopped;

  factory PlayEvent.disconnected() = Disconnected;

  factory PlayEvent.sliderDragChanged({
    required bool isSliding,
  }) = SliderDragChanged;

  factory PlayEvent.sliderValueChanged({
    required double newValue,
    @Default(false) bool triggerGoToSeconds,
  }) = SliderValueChanged;
}
