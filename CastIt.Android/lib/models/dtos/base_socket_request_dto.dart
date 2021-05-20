import 'package:json_annotation/json_annotation.dart';

part 'base_socket_request_dto.g.dart';

abstract class BaseSocketRequest {
  late String messageType;
}

@JsonSerializable()
class BaseSocketRequestDto implements BaseSocketRequest {
  @override
  @JsonKey(name: 'MessageType')
  String messageType;

  BaseSocketRequestDto({
    required this.messageType,
  });

  factory BaseSocketRequestDto.fromJson(Map<String, dynamic> json) => _$BaseSocketRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$BaseSocketRequestDtoToJson(this);
}

abstract class AbstractBaseSocketRequestDto implements BaseSocketRequest {
  @override
  @JsonKey(name: 'MessageType')
  late String messageType;

  AbstractBaseSocketRequestDto();
}
