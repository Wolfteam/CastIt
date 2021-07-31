import 'package:json_annotation/json_annotation.dart';

part 'set_file_options_request_dto.g.dart';

@JsonSerializable()
class SetFileOptionsRequestDto {
  final int streamIndex;
  final bool isAudio;
  final bool isSubTitle;
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
