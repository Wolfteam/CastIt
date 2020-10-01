import 'package:equatable/equatable.dart';
import 'package:json_annotation/json_annotation.dart';

part 'file_item_response_dto.g.dart';

@JsonSerializable()
class FileItemResponseDto extends Equatable {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'Position')
  final int position;

  @JsonKey(name: 'Path')
  final String path;

  @JsonKey(name: 'PlayedSeconds')
  final double playedSeconds;

  @JsonKey(name: 'TotalSeconds')
  final double totalSeconds;

  @JsonKey(name: 'PlayedPercentage')
  final double playedPercentage;

  @JsonKey(name: 'IsBeingPlayed')
  final bool isBeingPlayed;

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

  @JsonKey(name: 'Loop')
  final bool loop;

  @JsonKey(name: 'SubTitle')
  final String subtitle;

  @override
  List<Object> get props => [
        id,
        position,
        path,
        totalSeconds,
        playedPercentage,
        isBeingPlayed,
        playListId,
        isLocalFile,
        isUrlFile,
        exists,
        filename,
        size,
        ext,
        loop,
        subtitle,
        playedSeconds,
      ];

  const FileItemResponseDto({
    this.id,
    this.position,
    this.path,
    this.totalSeconds,
    this.playedPercentage,
    this.isBeingPlayed,
    this.playListId,
    this.isLocalFile,
    this.isUrlFile,
    this.exists,
    this.filename,
    this.size,
    this.ext,
    this.loop,
    this.subtitle,
    this.playedSeconds,
  });

  factory FileItemResponseDto.fromJson(Map<String, dynamic> json) => _$FileItemResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Position',
        'Path',
        'TotalSeconds',
        'PlayedPercentage',
        'IsBeingPlayed',
        'PlayListId',
        'IsLocalFile',
        'IsUrlFile',
        'Exists',
        'Filename',
        'Size',
        'Extension',
        'Loop',
        'SubTitle',
        'PlayedSeconds',
      ];
}
