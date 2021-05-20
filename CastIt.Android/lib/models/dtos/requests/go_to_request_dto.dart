import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'go_to_request_dto.g.dart';

@JsonSerializable()
class GoToRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Previous')
  final bool previous;

  @JsonKey(name: 'Next')
  final bool next;

  GoToRequestDto({
    required this.previous,
    required this.next,
  }) : super();

  factory GoToRequestDto.fromJson(Map<String, dynamic> json) => _$GoToRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$GoToRequestDtoToJson(this);
}
