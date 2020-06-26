import 'package:json_annotation/json_annotation.dart';

import 'app_response_dto.dart';
import 'json_generic_converter.dart';

part 'app_list_response_dto.g.dart';

@JsonSerializable()
class AppListResponseDto<T> extends AppResponseDto<List<T>> {
  AppListResponseDto() {
    result = <T>[];
  }

  factory AppListResponseDto.fromJson(Map<String, dynamic> json) =>
      _$AppListResponseDtoFromJson(json);
}
