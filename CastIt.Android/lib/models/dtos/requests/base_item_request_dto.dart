import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'base_item_request_dto.g.dart';

@JsonSerializable()
class BaseItemRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  BaseItemRequestDto({
    @required String msgType,
    @required this.id,
  }) : super(messageType: msgType);

  factory BaseItemRequestDto.fromJson(Map<String, dynamic> json) => _$BaseItemRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$BaseItemRequestDtoToJson(this);
}
