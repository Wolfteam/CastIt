import 'package:flutter/cupertino.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'go_to_seconds_request_dto.g.dart';

@JsonSerializable()
class GoToSecondsRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Seconds')
  final double seconds;
  GoToSecondsRequestDto({
    @required String msgType,
    @required this.seconds,
  }) : super(messageType: msgType);

  factory GoToSecondsRequestDto.fromJson(Map<String, dynamic> json) => _$GoToSecondsRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$GoToSecondsRequestDtoToJson(this);
}
