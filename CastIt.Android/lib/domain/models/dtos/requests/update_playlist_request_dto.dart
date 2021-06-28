import 'package:json_annotation/json_annotation.dart';

part 'update_playlist_request_dto.g.dart';

@JsonSerializable()
class UpdatePlayListRequestDto {
  final String name;

  UpdatePlayListRequestDto({
    required this.name,
  }) : super();

  factory UpdatePlayListRequestDto.fromJson(Map<String, dynamic> json) => _$UpdatePlayListRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$UpdatePlayListRequestDtoToJson(this);
}
