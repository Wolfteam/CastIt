import 'package:freezed_annotation/freezed_annotation.dart';

part 'volume_level_changed_response_dto.freezed.dart';
part 'volume_level_changed_response_dto.g.dart';

@freezed
abstract class VolumeLevelChangedResponseDto
    implements _$VolumeLevelChangedResponseDto {
  factory VolumeLevelChangedResponseDto({
    @required @JsonKey(name: 'VolumeLevel') double volumeLevel,
    @required @JsonKey(name: 'IsMuted') bool isMuted,
  }) = _VolumeLevelChangedResponseDto;

  factory VolumeLevelChangedResponseDto.fromJson(Map<String, dynamic> json) =>
      _$VolumeLevelChangedResponseDtoFromJson(json);

  static List<String> get jsonKeys => ['VolumeLevel', 'IsMuted'];
}
