// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'app_response_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AppResponseDto<T> _$AppResponseDtoFromJson<T>(Map<String, dynamic> json) {
  return AppResponseDto<T>(
    result: JsonGenericConverter<T>().fromJson(json['Result']),
  )
    ..succeed = json['Succeed'] as bool
    ..message = json['Message'] as String;
}

Map<String, dynamic> _$AppResponseDtoToJson<T>(AppResponseDto<T> instance) =>
    <String, dynamic>{
      'Succeed': instance.succeed,
      'Message': instance.message,
      'Result': JsonGenericConverter<T>().toJson(instance.result),
    };
