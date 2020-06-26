// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'file_response_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

FileResponseDto _$FileResponseDtoFromJson(Map<String, dynamic> json) {
  return FileResponseDto(
    id: json['Id'] as int,
    position: json['Position'] as int,
    path: json['Path'] as String,
    playedPercentage: (json['PlayedPercentage'] as num)?.toDouble(),
    playListId: json['PlayListId'] as int,
    isLocalFile: json['IsLocalFile'] as bool,
    isUrlFile: json['IsUrlFile'] as bool,
    exists: json['Exists'] as bool,
    filename: json['Filename'] as String,
    size: json['Size'] as String,
    ext: json['Extension'] as String,
  );
}

Map<String, dynamic> _$FileResponseDtoToJson(FileResponseDto instance) =>
    <String, dynamic>{
      'Id': instance.id,
      'Position': instance.position,
      'Path': instance.path,
      'PlayedPercentage': instance.playedPercentage,
      'PlayListId': instance.playListId,
      'IsLocalFile': instance.isLocalFile,
      'IsUrlFile': instance.isUrlFile,
      'Exists': instance.exists,
      'Filename': instance.filename,
      'Size': instance.size,
      'Extension': instance.ext,
    };
