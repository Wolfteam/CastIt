import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'set_volume_request_dto.g.dart';

@JsonSerializable()
class SetVolumeRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'VolumeLevel')
  final double volumeLevel;

  @JsonKey(name: 'IsMuted')
  final bool isMuted;

  SetVolumeRequestDto({
    @required String msgType,
    @required this.volumeLevel,
    @required this.isMuted,
  }) : super(messageType: msgType);

  factory SetVolumeRequestDto.fromJson(Map<String, dynamic> json) => _$SetVolumeRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$SetVolumeRequestDtoToJson(this);
}
