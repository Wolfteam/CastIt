import 'package:freezed_annotation/freezed_annotation.dart';

part 'volume_level_changed_response_dto.freezed.dart';
part 'volume_level_changed_response_dto.g.dart';

@freezed
sealed class VolumeLevelChangedResponseDto with _$VolumeLevelChangedResponseDto {
  factory VolumeLevelChangedResponseDto({required double volumeLevel, required bool isMuted}) = _VolumeLevelChangedResponseDto;

  factory VolumeLevelChangedResponseDto.fromJson(Map<String, dynamic> json) => _$VolumeLevelChangedResponseDtoFromJson(json);
}
