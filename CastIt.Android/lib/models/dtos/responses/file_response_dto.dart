import 'package:equatable/equatable.dart';
import 'package:json_annotation/json_annotation.dart';

part 'file_response_dto.g.dart';

@JsonSerializable()
class FileResponseDto extends Equatable {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'Position')
  final int position;

  @JsonKey(name: 'Path')
  final String path;

  @JsonKey(name: 'PlayedPercentage')
  final double playedPercentage;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  @JsonKey(name: 'IsLocalFile')
  final bool isLocalFile;

  @JsonKey(name: 'IsUrlFile')
  final bool isUrlFile;

  @JsonKey(name: 'Exists')
  final bool exists;

  @JsonKey(name: 'Filename')
  final String filename;

  @JsonKey(name: 'Size')
  final String size;

  @JsonKey(name: 'Extension')
  final String ext;

  @override
  List<Object> get props => [
        id,
        position,
        path,
        playedPercentage,
        playListId,
        isLocalFile,
        isUrlFile,
        exists,
        filename,
        size,
        ext
      ];

  const FileResponseDto({
    this.id,
    this.position,
    this.path,
    this.playedPercentage,
    this.playListId,
    this.isLocalFile,
    this.isUrlFile,
    this.exists,
    this.filename,
    this.size,
    this.ext,
  });

  factory FileResponseDto.fromJson(Map<String, dynamic> json) =>
      _$FileResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Position',
        'Path',
        'PlayedPercentage',
        'PlayListId',
        'IsLocalFile',
        'IsUrlFile',
        'Exists',
        'Filename',
        'Size',
        'Extension',
      ];
}
