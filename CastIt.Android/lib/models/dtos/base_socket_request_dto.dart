import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

part 'base_socket_request_dto.g.dart';

@JsonSerializable()
class BaseSocketRequestDto {
  @JsonKey(name: 'MessageType')
  String messageType;

  BaseSocketRequestDto({
    @required this.messageType,
  });

  factory BaseSocketRequestDto.fromJson(Map<String, dynamic> json) => _$BaseSocketRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$BaseSocketRequestDtoToJson(this);
}
