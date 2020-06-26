// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'playlist_response_dto.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

PlayListResponseDto _$PlayListResponseDtoFromJson(Map<String, dynamic> json) {
  return PlayListResponseDto(
    id: json['Id'] as int,
    name: json['Name'] as String,
    position: json['Position'] as int,
    loop: json['Loop'] as bool,
    shuffle: json['Shuffle'] as bool,
    numberOfFiles: json['NumberOfFiles'] as int,
  );
}

Map<String, dynamic> _$PlayListResponseDtoToJson(
        PlayListResponseDto instance) =>
    <String, dynamic>{
      'Id': instance.id,
      'Name': instance.name,
      'Position': instance.position,
      'Loop': instance.loop,
      'Shuffle': instance.shuffle,
      'NumberOfFiles': instance.numberOfFiles,
    };
