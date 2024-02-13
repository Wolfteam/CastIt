part of 'played_file_options_bloc.dart';

@freezed
class PlayedFileOptionsState with _$PlayedFileOptionsState {
  const factory PlayedFileOptionsState.loaded({
    required List<FileItemOptionsResponseDto> options,
    @Default(AppConstants.maxVolumeLevel) double volumeLvl,
    @Default(false) bool isMuted,
    @Default(false) bool isDraggingVolumeSlider,
  }) = _LoadedState;

  const factory PlayedFileOptionsState.closed() = _ClosedState;
}
