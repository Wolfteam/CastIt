import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'delete_playlist_request_dto.g.dart';

@JsonSerializable()
class DeletePlayListRequestDto extends AbstractBaseSocketRequestDto {
  final int id;

  DeletePlayListRequestDto({
    required this.id,
  }) : super();

  factory DeletePlayListRequestDto.fromJson(Map<String, dynamic> json) => _$DeletePlayListRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$DeletePlayListRequestDtoToJson(this);
}
