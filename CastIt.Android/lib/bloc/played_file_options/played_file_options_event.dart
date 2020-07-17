part of 'played_file_options_bloc.dart';

@freezed
abstract class PlayedFileOptionsEvent implements _$PlayedFileOptionsEvent {
  factory PlayedFileOptionsEvent.load({
    @required int id,
  }) = PlayedFileOptionsLoadEvent;

  factory PlayedFileOptionsEvent.loaded({
    @required List<FileItemOptionsResponseDto> options,
  }) = PlayedFileOptionsLoadedEvent;

  factory PlayedFileOptionsEvent.setFileOption({
    @required int streamIndex,
    @required bool isAudio,
    @required bool isSubtitle,
    @required bool isQuality,
  }) = PlayedFileOptionsSetEvent;

  factory PlayedFileOptionsEvent.volumeChanged({
    @required double volumeLvl,
    @required bool isMuted,
  }) = PlayedFileOptionsVolumeLevelChangedEvent;

  factory PlayedFileOptionsEvent.setVolume({
    @required double volumeLvl,
    @required bool isMuted,
  }) = PlayedFileOptionsSetVolumeEvent;

  factory PlayedFileOptionsEvent.closeModal() = PlayedFileOptionsCloseModalEvent;
}
