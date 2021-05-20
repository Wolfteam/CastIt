import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'delete_file_request_dto.g.dart';

@JsonSerializable()
class DeleteFileRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  DeleteFileRequestDto({
    required this.id,
    required this.playListId,
  }) : super();

  factory DeleteFileRequestDto.fromJson(Map<String, dynamic> json) => _$DeleteFileRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$DeleteFileRequestDtoToJson(this);
}
