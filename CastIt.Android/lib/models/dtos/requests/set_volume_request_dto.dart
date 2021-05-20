import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'set_volume_request_dto.g.dart';

@JsonSerializable()
class SetVolumeRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'VolumeLevel')
  final double volumeLevel;

  @JsonKey(name: 'IsMuted')
  final bool isMuted;

  SetVolumeRequestDto({
    required this.volumeLevel,
    required this.isMuted,
  }) : super();

  factory SetVolumeRequestDto.fromJson(Map<String, dynamic> json) => _$SetVolumeRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetVolumeRequestDtoToJson(this);
}
