import 'package:json_annotation/json_annotation.dart';

part 'set_volume_request_dto.g.dart';

@JsonSerializable()
class SetVolumeRequestDto {
  final double volumeLevel;
  final bool isMuted;

  SetVolumeRequestDto({
    required this.volumeLevel,
    required this.isMuted,
  }) : super();

  factory SetVolumeRequestDto.fromJson(Map<String, dynamic> json) => _$SetVolumeRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetVolumeRequestDtoToJson(this);
}
