import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'go_to_request_dto.g.dart';

@JsonSerializable()
class GoToRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Previous')
  final bool previous;

  @JsonKey(name: 'Next')
  final bool next;

  GoToRequestDto({
    @required String msgType,
    @required this.previous,
    @required this.next,
  }) : super(messageType: msgType);

  factory GoToRequestDto.fromJson(Map<String, dynamic> json) => _$GoToRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$GoToRequestDtoToJson(this);
}
