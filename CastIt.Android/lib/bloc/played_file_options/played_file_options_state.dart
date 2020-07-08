part of 'played_file_options_bloc.dart';

@freezed
abstract class PlayedFileOptionsState implements _$PlayedFileOptionsState {
  factory PlayedFileOptionsState.loading() = PlayedFileOptionsLoadingState;
  factory PlayedFileOptionsState.loaded({
    @required List<FileItemOptionsResponseDto> options,
  }) = PlayedFileOptionsLoadedState;
}
