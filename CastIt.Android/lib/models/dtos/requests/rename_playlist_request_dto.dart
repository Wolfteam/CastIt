import 'package:json_annotation/json_annotation.dart';

import './base_item_request_dto.dart';

part 'rename_playlist_request_dto.g.dart';

@JsonSerializable()
class RenamePlayListRequestDto extends AbstractBaseItemRequestDto {
  @JsonKey(name: 'Name')
  final String name;

  RenamePlayListRequestDto({
    required this.name,
  }) : super();

  factory RenamePlayListRequestDto.fromJson(Map<String, dynamic> json) => _$RenamePlayListRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$RenamePlayListRequestDtoToJson(this);
}
