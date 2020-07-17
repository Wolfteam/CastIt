import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../common/enums/video_scale_type.dart';

part 'app_settings_response_dto.freezed.dart';
part 'app_settings_response_dto.g.dart';

@freezed
abstract class AppSettingsResponseDto implements _$AppSettingsResponseDto {
  VideoScaleType get videoScaleType => getVideoScaleType(videoScale);

  factory AppSettingsResponseDto({
    @required @JsonKey(name: 'StartFilesFromTheStart') bool playFromTheStart,
    @required @JsonKey(name: 'PlayNextFileAutomatically') bool playNextFileAutomatically,
    @required @JsonKey(name: 'ForceVideoTranscode') bool forceVideoTranscode,
    @required @JsonKey(name: 'ForceAudioTranscode') bool forceAudioTranscode,
    @required @JsonKey(name: 'VideoScale') int videoScale,
    @required @JsonKey(name: 'EnableHardwareAcceleration') bool enableHwAccel,
  }) = _AppSettingsResponseDto;

  factory AppSettingsResponseDto.fromJson(Map<String, dynamic> json) => _$AppSettingsResponseDtoFromJson(json);

  AppSettingsResponseDto._();

  static List<String> get jsonKeys => [
        'StartFilesFromTheStart',
        'PlayNextFileAutomatically',
        'ForceVideoTranscode',
        'ForceAudioTranscode',
        'VideoScale',
        'EnableHardwareAcceleration',
      ];
}
