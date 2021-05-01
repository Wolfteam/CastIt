import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

part 'file_item_response_dto.freezed.dart';
part 'file_item_response_dto.g.dart';

@freezed
abstract class FileItemResponseDto implements _$FileItemResponseDto {
  const factory FileItemResponseDto({
    @JsonKey(name: 'Id') @required int id,
    @JsonKey(name: 'Position') @required int position,
    @JsonKey(name: 'Path') @required String path,
    @JsonKey(name: 'PlayedSeconds') @required double playedSeconds,
    @JsonKey(name: 'TotalSeconds') @required double totalSeconds,
    @JsonKey(name: 'PlayedPercentage') @required double playedPercentage,
    @JsonKey(name: 'IsBeingPlayed') @required bool isBeingPlayed,
    @JsonKey(name: 'PlayListId') @required int playListId,
    @JsonKey(name: 'IsLocalFile') @required bool isLocalFile,
    @JsonKey(name: 'IsUrlFile') @required bool isUrlFile,
    @JsonKey(name: 'Exists') @required bool exists,
    @JsonKey(name: 'Filename') @required String filename,
    @JsonKey(name: 'Size') @required String size,
    @JsonKey(name: 'Extension') @required String ext,
    @JsonKey(name: 'Loop') @required bool loop,
    @JsonKey(name: 'SubTitle') @required String subtitle,
  }) = _FileItemResponseDto;

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
