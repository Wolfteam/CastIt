part of 'play_bloc.dart';

@freezed
sealed class PlayEvent with _$PlayEvent {
  const factory PlayEvent.connected() = PlayEventConnected;

  const factory PlayEvent.fileLoading() = PlayEventFileLoading;

  const factory PlayEvent.fileLoadingError({required String msg}) = PlayEventFileLoadingError;

  const factory PlayEvent.fileLoaded({required PlayedFile file}) = PlayEventFileLoaded;

  const factory PlayEvent.fileChanged({required FileItemResponseDto file}) = PlayEventFileChanged;

  const factory PlayEvent.playListChanged({required GetAllPlayListResponseDto playList}) = PlayEventPlayListChanged;

  const factory PlayEvent.timeChanged({required double seconds}) = PlayEventTimeChanged;

  const factory PlayEvent.paused() = PlayEventPaused;

  const factory PlayEvent.stopped() = PlayEventStopped;

  const factory PlayEvent.disconnected() = PlayEventDisconnected;

  const factory PlayEvent.sliderDragChanged({required bool isSliding}) = PlayEventSliderDragChanged;

  const factory PlayEvent.sliderValueChanged({required double newValue, @Default(false) bool triggerGoToSeconds}) =
      PlayEventSliderValueChanged;
}
