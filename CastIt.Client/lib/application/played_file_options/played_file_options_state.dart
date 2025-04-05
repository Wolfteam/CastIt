part of 'played_file_options_bloc.dart';

@freezed
sealed class PlayedFileOptionsState with _$PlayedFileOptionsState {
  const factory PlayedFileOptionsState.loaded({
    required List<FileItemOptionsResponseDto> options,
    @Default(AppConstants.maxVolumeLevel) double volumeLvl,
    @Default(false) bool isMuted,
    @Default(false) bool isDraggingVolumeSlider,
  }) = PlayedFileOptionsStateLoadedState;

  const factory PlayedFileOptionsState.closed() = PlayedFileOptionsStateClosedState;
}
