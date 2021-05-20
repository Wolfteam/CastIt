import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../common/enums/video_scale_type.dart';

part 'app_settings_response_dto.freezed.dart';
part 'app_settings_response_dto.g.dart';

@freezed
abstract class AppSettingsResponseDto implements _$AppSettingsResponseDto {
  VideoScaleType get videoScaleType => getVideoScaleType(videoScale);

  factory AppSettingsResponseDto({
    @JsonKey(name: 'StartFilesFromTheStart') required bool playFromTheStart,
    @JsonKey(name: 'PlayNextFileAutomatically') required bool playNextFileAutomatically,
    @JsonKey(name: 'ForceVideoTranscode') required bool forceVideoTranscode,
    @JsonKey(name: 'ForceAudioTranscode') required bool forceAudioTranscode,
    @JsonKey(name: 'VideoScale') required int videoScale,
    @JsonKey(name: 'EnableHardwareAcceleration') required bool enableHwAccel,
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
