import 'package:freezed_annotation/freezed_annotation.dart';

part 'player_status_response_dto.freezed.dart';
part 'player_status_response_dto.g.dart';

@freezed
class PlayerStatusResponseDto with _$PlayerStatusResponseDto {
  factory PlayerStatusResponseDto({
    String? playFromTheStart,
    required bool isPlaying,
    required bool isPaused,
    required bool isPlayingOrPaused,
    required double currentMediaDuration,
    required double elapsedSeconds,
    required double playedPercentage,
    required double volumeLevel,
    required bool isMuted,
  }) = _PlayerStatusResponseDto;

  factory PlayerStatusResponseDto.fromJson(Map<String, dynamic> json) => _$PlayerStatusResponseDtoFromJson(json);
}
