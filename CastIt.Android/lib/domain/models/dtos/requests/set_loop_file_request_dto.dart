import 'package:json_annotation/json_annotation.dart';

part 'set_loop_file_request_dto.g.dart';

@JsonSerializable()
class SetLoopFileRequestDto {
  final int id;
  final int playListId;
  final bool loop;

  SetLoopFileRequestDto({
    required this.id,
    required this.playListId,
    required this.loop,
  }) : super();

  factory SetLoopFileRequestDto.fromJson(Map<String, dynamic> json) => _$SetLoopFileRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetLoopFileRequestDtoToJson(this);
}
