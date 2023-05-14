part of 'played_file_options_bloc.dart';

@freezed
class PlayedFileOptionsEvent with _$PlayedFileOptionsEvent {
  factory PlayedFileOptionsEvent.loaded({
    required List<FileItemOptionsResponseDto> options,
  }) = _Loaded;

  factory PlayedFileOptionsEvent.setFileOption({
    required int streamIndex,
    required bool isAudio,
    required bool isSubtitle,
    required bool isQuality,
  }) = _SetFileOption;

  factory PlayedFileOptionsEvent.volumeChanged({
    required double volumeLvl,
    required bool isMuted,
  }) = _VolumeChanged;

  factory PlayedFileOptionsEvent.setVolume({
    required double volumeLvl,
    required bool isMuted,
    required bool triggerChange,
  }) = _SetVolume;

  factory PlayedFileOptionsEvent.volumeSliderDragStarted() = _VolumeSliderDragStarted;

  factory PlayedFileOptionsEvent.closeModal() = _CloseModal;
}
