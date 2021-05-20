import 'package:freezed_annotation/freezed_annotation.dart';

part 'volume_level_changed_response_dto.freezed.dart';
part 'volume_level_changed_response_dto.g.dart';

@freezed
abstract class VolumeLevelChangedResponseDto implements _$VolumeLevelChangedResponseDto {
  factory VolumeLevelChangedResponseDto({
    @JsonKey(name: 'VolumeLevel') required double volumeLevel,
    @JsonKey(name: 'IsMuted') required bool isMuted,
  }) = _VolumeLevelChangedResponseDto;

  factory VolumeLevelChangedResponseDto.fromJson(Map<String, dynamic> json) => _$VolumeLevelChangedResponseDtoFromJson(json);

  static List<String> get jsonKeys => ['VolumeLevel', 'IsMuted'];
}
