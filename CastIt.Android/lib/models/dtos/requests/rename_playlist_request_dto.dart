import 'package:flutter/foundation.dart';
import 'package:json_annotation/json_annotation.dart';

import './base_item_request_dto.dart';

part 'rename_playlist_request_dto.g.dart';

@JsonSerializable()
class RenamePlayListRequestDto extends BaseItemRequestDto {
  @JsonKey(name: 'Name')
  final String name;

  RenamePlayListRequestDto({
    @required String msgType,
    @required int id,
    this.name,
  }) : super(msgType: msgType, id: id);

  factory RenamePlayListRequestDto.fromJson(Map<String, dynamic> json) => _$RenamePlayListRequestDtoFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$RenamePlayListRequestDtoToJson(this);
}
