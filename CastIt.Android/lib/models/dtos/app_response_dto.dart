import 'package:json_annotation/json_annotation.dart';

import '../../common/utils/json_generic_converter.dart';
import 'empty_response_dto.dart';

part 'app_response_dto.g.dart';

@JsonSerializable()
class AppResponseDto<T> extends EmptyResponseDto {
  @JsonGenericConverter()
  @JsonKey(name: 'Result')
  T? result;

  AppResponseDto({
    this.result,
  });

  factory AppResponseDto.fromJson(Map<String, dynamic> json) => _$AppResponseDtoFromJson(json);
}
