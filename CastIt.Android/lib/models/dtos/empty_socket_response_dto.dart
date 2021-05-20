import 'package:json_annotation/json_annotation.dart';

import 'empty_response_dto.dart';

part 'empty_socket_response_dto.g.dart';

@JsonSerializable()
class EmptySocketResponseDto extends EmptyResponseDto {
  @JsonKey(name: 'MessageType')
  final String? messageType;

  EmptySocketResponseDto({
    this.messageType,
  });

  factory EmptySocketResponseDto.fromJson(Map<String, dynamic> json) => _$EmptySocketResponseDtoFromJson(json);
}
