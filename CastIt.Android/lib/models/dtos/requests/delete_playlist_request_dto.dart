import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'delete_playlist_request_dto.g.dart';

@JsonSerializable()
class DeletePlayListRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  DeletePlayListRequestDto({
    @required String msgType,
    @required this.id,
  }) : super(messageType: msgType);

  factory DeletePlayListRequestDto.fromJson(Map<String, dynamic> json) => _$DeletePlayListRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$DeletePlayListRequestDtoToJson(this);
}
