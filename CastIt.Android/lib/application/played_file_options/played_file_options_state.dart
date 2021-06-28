part of 'played_file_options_bloc.dart';

@freezed
class PlayedFileOptionsState with _$PlayedFileOptionsState {
  const factory PlayedFileOptionsState.loaded({
    required List<FileItemOptionsResponseDto> options,
    @Default(1.0) double volumeLvl,
    @Default(false) bool isMuted,
  }) = _LoadedState;

  const factory PlayedFileOptionsState.closed() = _ClosedState;
}
