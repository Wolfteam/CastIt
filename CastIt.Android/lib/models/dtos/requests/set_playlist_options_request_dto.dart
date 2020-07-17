import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'set_playlist_options_request_dto.g.dart';

@JsonSerializable()
class SetPlayListOptionsRequestDto extends BaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'Loop')
  final bool loop;

  @JsonKey(name: 'Shuffle')
  final bool shuffle;

  SetPlayListOptionsRequestDto({
    @required String msgType,
    @required this.id,
    @required this.loop,
    @required this.shuffle,
  }) : super(messageType: msgType);

  factory SetPlayListOptionsRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SetPlayListOptionsRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$SetPlayListOptionsRequestDtoToJson(this);
}
