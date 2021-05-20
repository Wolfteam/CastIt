import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'base_item_request_dto.g.dart';

@JsonSerializable()
class BaseItemRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  BaseItemRequestDto({
    required this.id,
  }) : super();

  factory BaseItemRequestDto.fromJson(Map<String, dynamic> json) => _$BaseItemRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$BaseItemRequestDtoToJson(this);
}

abstract class AbstractBaseItemRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Id')
  late int id;

  AbstractBaseItemRequestDto() : super();
}
