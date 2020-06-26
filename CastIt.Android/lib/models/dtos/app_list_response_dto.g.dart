// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_list_response_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AppListResponseDto<T> _$AppListResponseDtoFromJson<T>(
    Map<String, dynamic> json) {
  return AppListResponseDto<T>()
    ..succeed = json['Succeed'] as bool
    ..message = json['Message'] as String
    ..result = (json['Result'] as List)
        ?.map(JsonGenericConverter<T>().fromJson)
        ?.toList();
}

Map<String, dynamic> _$AppListResponseDtoToJson<T>(
        AppListResponseDto<T> instance) =>
    <String, dynamic>{
      'Succeed': instance.succeed,
      'Message': instance.message,
      'Result':
          instance.result?.map(JsonGenericConverter<T>().toJson)?.toList(),
    };
