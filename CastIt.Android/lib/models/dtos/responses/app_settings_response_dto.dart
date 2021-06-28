import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../common/enums/video_scale_type.dart';

part 'app_settings_response_dto.freezed.dart';
part 'app_settings_response_dto.g.dart';

//TODO: RENAME TO SERVER SETTINGS
@freezed
class AppSettingsResponseDto with _$AppSettingsResponseDto {
  VideoScaleType get videoScaleType => getVideoScaleType(videoScale);

  factory AppSettingsResponseDto({
    @JsonKey(name: 'startFilesFromTheStart') required bool playFromTheStart,
    @JsonKey(name: 'playNextFileAutomatically') required bool playNextFileAutomatically,
    @JsonKey(name: 'forceVideoTranscode') required bool forceVideoTranscode,
    @JsonKey(name: 'forceAudioTranscode') required bool forceAudioTranscode,
    @JsonKey(name: 'videoScale') required int videoScale,
    @JsonKey(name: 'enableHardwareAcceleration') required bool enableHwAccel,
    @JsonKey(name: 'loadFirstSubtitleFoundAutomatically') required bool loadFirstSubtitleFoundAutomatically,
  }) = _AppSettingsResponseDto;

  factory AppSettingsResponseDto.fromJson(Map<String, dynamic> json) => _$AppSettingsResponseDtoFromJson(json);

  AppSettingsResponseDto._();

  static List<String> get jsonKeys => [
        'startFilesFromTheStart',
        'playNextFileAutomatically',
        'forceVideoTranscode',
        'forceAudioTranscode',
        'videoScale',
        'enableHardwareAcceleration',
        'loadFirstSubtitleFoundAutomatically',
      ];
}
