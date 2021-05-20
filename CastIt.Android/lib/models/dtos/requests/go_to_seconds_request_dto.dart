import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'go_to_seconds_request_dto.g.dart';

@JsonSerializable()
class GoToSecondsRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Seconds')
  final double seconds;

  GoToSecondsRequestDto({
    required this.seconds,
  }) : super();

  factory GoToSecondsRequestDto.fromJson(Map<String, dynamic> json) => _$GoToSecondsRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$GoToSecondsRequestDtoToJson(this);
}
