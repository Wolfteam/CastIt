import 'package:json_annotation/json_annotation.dart';

part 'empty_response_dto.g.dart';

@JsonSerializable()
class EmptyResponseDto {
  @JsonKey(name: 'Succeed')
  bool succeed = false;

  @JsonKey(name: 'Message')
  String message;

  EmptyResponseDto({
    this.succeed,
    this.message,
  }) {
    succeed ??= false;
  }

  factory EmptyResponseDto.fromJson(Map<String, dynamic> json) =>
      _$EmptyResponseDtoFromJson(json);
}
