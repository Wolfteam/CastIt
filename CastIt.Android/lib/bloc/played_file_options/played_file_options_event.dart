part of 'played_file_options_bloc.dart';

@freezed
abstract class PlayedFileOptionsEvent implements _$PlayedFileOptionsEvent {
  factory PlayedFileOptionsEvent.load({
    @required int id,
  }) = PlayedFileOptionsLoadEvent;

  factory PlayedFileOptionsEvent.loaded({
    @required List<FileItemOptionsResponseDto> options,
  }) = PlayedFileOptionsLoadedEvent;
}
