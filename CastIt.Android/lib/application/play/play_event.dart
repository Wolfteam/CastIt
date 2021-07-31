part of 'play_bloc.dart';

@freezed
class PlayEvent with _$PlayEvent {
  factory PlayEvent.connected() = _Connected;

  factory PlayEvent.fileLoading() = _FileLoading;

  factory PlayEvent.fileLoadingError({required String msg}) = _FileLoadingError;

  factory PlayEvent.fileLoaded({
    required PlayedFile file,
  }) = _FileLoaded;

  factory PlayEvent.timeChanged({
    required double seconds,
  }) = _TimeChanged;

  factory PlayEvent.paused() = _Paused;

  factory PlayEvent.stopped() = _Stopped;

  factory PlayEvent.disconnected() = _Disconnected;

  factory PlayEvent.sliderDragChanged({
    required bool isSliding,
  }) = _SliderDragChanged;

  factory PlayEvent.sliderValueChanged({
    required double newValue,
    @Default(false) bool triggerGoToSeconds,
  }) = _SliderValueChanged;
}
