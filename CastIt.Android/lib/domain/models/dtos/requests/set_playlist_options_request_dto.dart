import 'package:json_annotation/json_annotation.dart';

part 'set_playlist_options_request_dto.g.dart';

@JsonSerializable()
class SetPlayListOptionsRequestDto {
  final bool loop;
  final bool shuffle;

  SetPlayListOptionsRequestDto({
    required this.loop,
    required this.shuffle,
  }) : super();

  factory SetPlayListOptionsRequestDto.fromJson(Map<String, dynamic> json) => _$SetPlayListOptionsRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetPlayListOptionsRequestDtoToJson(this);
}
