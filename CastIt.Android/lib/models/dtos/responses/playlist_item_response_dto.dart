import 'package:json_annotation/json_annotation.dart';

import 'file_item_response_dto.dart';
import 'get_all_playlist_response_dto.dart';

part 'playlist_item_response_dto.g.dart';

@JsonSerializable()
class PlayListItemResponseDto extends GetAllPlayListResponseDto {
  @JsonKey(name: 'Files')
  final List<FileItemResponseDto> files;

  @override
  List<Object> get props => [
        id,
        name,
        position,
        loop,
        shuffle,
        numberOfFiles,
        files,
        totalDuration,
      ];

  const PlayListItemResponseDto({
    int id,
    String name,
    int position,
    bool loop,
    bool shuffle,
    int numberOfFiles,
    String totalDuration,
    this.files,
  }) : super(
          id: id,
          name: name,
          position: position,
          loop: loop,
          shuffle: shuffle,
          numberOfFiles: numberOfFiles,
          totalDuration: totalDuration,
        );

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
