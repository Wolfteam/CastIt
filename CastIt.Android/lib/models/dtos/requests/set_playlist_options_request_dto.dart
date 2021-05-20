import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'set_playlist_options_request_dto.g.dart';

@JsonSerializable()
class SetPlayListOptionsRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'Id')
  final int id;

  @JsonKey(name: 'Loop')
  final bool loop;

  @JsonKey(name: 'Shuffle')
  final bool shuffle;

  SetPlayListOptionsRequestDto({
    required this.id,
    required this.loop,
    required this.shuffle,
  }) : super();

  factory SetPlayListOptionsRequestDto.fromJson(Map<String, dynamic> json) => _$SetPlayListOptionsRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetPlayListOptionsRequestDtoToJson(this);
}
