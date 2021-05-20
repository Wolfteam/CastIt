import 'package:castit/models/dtos/base_socket_request_dto.dart';
import 'package:json_annotation/json_annotation.dart';

part 'play_file_request_dto.g.dart';

@JsonSerializable()
class PlayFileRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  @JsonKey(name: 'Force')
  final bool force;

  PlayFileRequestDto({
    required this.id,
    required this.playListId,
    required this.force,
  }) : super();

  factory PlayFileRequestDto.fromJson(Map<String, dynamic> json) => _$PlayFileRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$PlayFileRequestDtoToJson(this);
}
