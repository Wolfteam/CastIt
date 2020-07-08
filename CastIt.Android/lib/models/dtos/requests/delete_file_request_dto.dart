import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'delete_file_request_dto.g.dart';

@JsonSerializable()
class DeleteFileRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  DeleteFileRequestDto({
    @required String msgType,
    @required this.id,
    @required this.playListId,
  }) : super(messageType: msgType);

  factory DeleteFileRequestDto.fromJson(Map<String, dynamic> json) => _$DeleteFileRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$DeleteFileRequestDtoToJson(this);
}
