import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:json_annotation/json_annotation.dart';

import 'file_item_response_dto.dart';

part 'playlist_item_response_dto.freezed.dart';
part 'playlist_item_response_dto.g.dart';

@freezed
abstract class PlayListItemResponseDto implements _$PlayListItemResponseDto {
  const factory PlayListItemResponseDto({
    @JsonKey(name: 'Id') required int id,
    @JsonKey(name: 'Name') required String name,
    @JsonKey(name: 'Position') required int position,
    @JsonKey(name: 'Loop') required bool loop,
    @JsonKey(name: 'Shuffle') required bool shuffle,
    @JsonKey(name: 'NumberOfFiles') required int numberOfFiles,
    @JsonKey(name: 'TotalDuration') required String totalDuration,
    @Default(<FileItemResponseDto>[]) @JsonKey(name: 'Files') List<FileItemResponseDto> files,
  }) = _PlayListItemResponseDto;

  factory PlayListItemResponseDto.fromJson(Map<String, dynamic> json) => _$PlayListItemResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Name',
        'Position',
        'Loop',
        'Shuffle',
        'NumberOfFiles',
        'TotalDuration',
        'Files',
      ];
}
