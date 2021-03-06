import 'package:json_annotation/json_annotation.dart';

import '../base_socket_request_dto.dart';

part 'app_settings_request_dto.g.dart';

@JsonSerializable()
class AppSettingsRequestDto extends AbstractBaseSocketRequestDto {
  @JsonKey(name: 'StartFilesFromTheStart')
  final bool playFromTheStart;

  @JsonKey(name: 'PlayNextFileAutomatically')
  final bool playNextFileAutomatically;

  @JsonKey(name: 'ForceVideoTranscode')
  final bool forceVideoTranscode;

  @JsonKey(name: 'ForceAudioTranscode')
  final bool forceAudioTranscode;

  @JsonKey(name: 'VideoScale')
  final int videoScale;

  @JsonKey(name: 'EnableHardwareAcceleration')
  final bool enableHwAccel;

  AppSettingsRequestDto({
    required this.playFromTheStart,
    required this.playNextFileAutomatically,
    required this.forceAudioTranscode,
    required this.forceVideoTranscode,
    required this.videoScale,
    required this.enableHwAccel,
  }) : super();

  factory AppSettingsRequestDto.fromJson(Map<String, dynamic> json) => _$AppSettingsRequestDtoFromJson(json);

  Map<String, dynamic> toJson() => _$AppSettingsRequestDtoToJson(this);
}
