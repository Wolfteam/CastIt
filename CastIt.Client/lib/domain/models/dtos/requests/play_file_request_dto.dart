import 'package:json_annotation/json_annotation.dart';

part 'play_file_request_dto.g.dart';

@JsonSerializable()
class PlayFileRequestDto {
  final int id;
  final int playListId;
  final bool force;

  PlayFileRequestDto({
    required this.id,
    required this.playListId,
    required this.force,
  }) : super();

  factory PlayFileRequestDto.fromJson(Map<String, dynamic> json) => _$PlayFileRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$PlayFileRequestDtoToJson(this);
}
