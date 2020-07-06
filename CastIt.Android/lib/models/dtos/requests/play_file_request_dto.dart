import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import 'package:castit/models/dtos/base_socket_request_dto.dart';

part 'play_file_request_dto.g.dart';

@JsonSerializable()
class PlayFileRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'PlayListId')
  final int playListId;

  PlayFileRequestDto({
    @required String msgType,
    @required this.id,
    @required this.playListId,
  }) : super(messageType: msgType);

  factory PlayFileRequestDto.fromJson(Map<String, dynamic> json) => _$PlayFileRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$PlayFileRequestDtoToJson(this);
}
