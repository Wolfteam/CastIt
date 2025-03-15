part of 'played_file_options_bloc.dart';

@freezed
sealed class PlayedFileOptionsEvent with _$PlayedFileOptionsEvent {
  const factory PlayedFileOptionsEvent.loaded({required List<FileItemOptionsResponseDto> options}) = PlayedFileOptionsEventLoaded;

  const factory PlayedFileOptionsEvent.setFileOption({
    required int streamIndex,
    required bool isAudio,
    required bool isSubtitle,
    required bool isQuality,
  }) = PlayedFileOptionsEventSetFileOption;

  const factory PlayedFileOptionsEvent.volumeChanged({required double volumeLvl, required bool isMuted}) =
      PlayedFileOptionsEventVolumeChanged;

  const factory PlayedFileOptionsEvent.setVolume({
    required double volumeLvl,
    required bool isMuted,
    required bool triggerChange,
  }) = PlayedFileOptionsEventSetVolume;

  const factory PlayedFileOptionsEvent.volumeSliderDragStarted() = PlayedFileOptionsEventVolumeSliderDragStarted;

  const factory PlayedFileOptionsEvent.closeModal() = PlayedFileOptionsEventCloseModal;
}
