import 'package:json_annotation/json_annotation.dart';

import '../../common/utils/json_generic_converter.dart';
import 'empty_response_dto.dart';

part 'socket_response_dto.g.dart';

@JsonSerializable()
class SocketResponseDto<T> extends EmptyResponseDto {
  @JsonKey(name: 'MessageType')
  final String messageType;

  @JsonGenericConverter()
  @JsonKey(name: 'Result')
  T result;

  SocketResponseDto({
    this.messageType,
    this.result,
  });

  factory SocketResponseDto.fromJson(Map<String, dynamic> json) =>
      _$SocketResponseDtoFromJson(json);
}
