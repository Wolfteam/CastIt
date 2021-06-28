import 'package:json_annotation/json_annotation.dart';

part 'set_loop_file_request_dto.g.dart';

@JsonSerializable()
class SetLoopFileRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  @JsonKey(name: 'Loop')
  final bool loop;

  SetLoopFileRequestDto({
    required this.id,
    required this.playListId,
    required this.loop,
  }) : super();

  factory SetLoopFileRequestDto.fromJson(Map<String, dynamic> json) => _$SetLoopFileRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetLoopFileRequestDtoToJson(this);
}
