// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'empty_response_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

EmptyResponseDto _$EmptyResponseDtoFromJson(Map<String, dynamic> json) {
  return EmptyResponseDto(
    succeed: json['Succeed'] as bool,
    message: json['Message'] as String,
  );
}

Map<String, dynamic> _$EmptyResponseDtoToJson(EmptyResponseDto instance) =>
    <String, dynamic>{
      'Succeed': instance.succeed,
      'Message': instance.message,
    };
