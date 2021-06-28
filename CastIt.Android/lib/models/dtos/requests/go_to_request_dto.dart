import 'package:json_annotation/json_annotation.dart';

part 'go_to_request_dto.g.dart';

@JsonSerializable()
class GoToRequestDto {
  final bool previous;
  final bool next;

  GoToRequestDto({
    required this.previous,
    required this.next,
  }) : super();

  factory GoToRequestDto.fromJson(Map<String, dynamic> json) => _$GoToRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$GoToRequestDtoToJson(this);
}
