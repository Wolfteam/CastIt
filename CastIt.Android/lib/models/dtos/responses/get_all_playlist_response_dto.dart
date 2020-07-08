import 'package:equatable/equatable.dart';
import 'package:json_annotation/json_annotation.dart';

part 'get_all_playlist_response_dto.g.dart';

@JsonSerializable()
class GetAllPlayListResponseDto extends Equatable {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'Name')
  final String name;

  @JsonKey(name: 'Position')
  final int position;

  @JsonKey(name: 'Loop')
  final bool loop;

  @JsonKey(name: 'Shuffle')
  final bool shuffle;

  @JsonKey(name: 'NumberOfFiles')
  final int numberOfFiles;

  @override
  List<Object> get props => [
        id,
        name,
        position,
        loop,
        shuffle,
        numberOfFiles,
      ];

  const GetAllPlayListResponseDto({
    this.id,
    this.name,
    this.position,
    this.loop,
    this.shuffle,
    this.numberOfFiles,
  });

  factory GetAllPlayListResponseDto.fromJson(Map<String, dynamic> json) => _$GetAllPlayListResponseDtoFromJson(json);

  static List<String> get jsonKeys => [
        'Id',
        'Name',
        'Position',
        'Loop',
        'Shuffle',
        'NumberOfFiles',
      ];
}
