import 'package:castit/models/dtos/base_socket_request_dto.dart';
import 'package:json_annotation/json_annotation.dart';

part 'set_file_options_request_dto.g.dart';

@JsonSerializable()
class SetFileOptionsRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'StreamIndex')
  final int streamIndex;

  @JsonKey(name: 'IsAudio')
  final bool isAudio;

  @JsonKey(name: 'IsSubTitle')
  final bool isSubTitle;

  @JsonKey(name: 'IsQuality')
  final bool isQuality;

  SetFileOptionsRequestDto({
    required this.streamIndex,
    required this.isAudio,
    required this.isSubTitle,
    required this.isQuality,
  });

  factory SetFileOptionsRequestDto.fromJson(Map<String, dynamic> json) => _$SetFileOptionsRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$SetFileOptionsRequestDtoToJson(this);
}
