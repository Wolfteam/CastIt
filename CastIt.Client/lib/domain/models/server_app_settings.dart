import 'package:castit/domain/enums/enums.dart';
import 'package:json_annotation/json_annotation.dart';

part 'server_app_settings.g.dart';

@JsonSerializable()
class ServerAppSettings {
  VideoScaleType get videoScaleType => getVideoScaleType(videoScale);

  WebVideoQualityType get webVideoQualityType => getWebVideoQualityType(webVideoQuality);

  SubtitleFgColorType get currentSubtitleFgColorType => SubtitleFgColorType.values[currentSubtitleFgColor];

  SubtitleBgColorType get currentSubtitleBgColorType => SubtitleBgColorType.values[currentSubtitleBgColor];

  SubtitleFontScaleType get currentSubtitleFontScaleType => getSubtitleFontScaleType(currentSubtitleFontScale);

  TextTrackFontStyleType get currentSubtitleFontStyleType => TextTrackFontStyleType.values[currentSubtitleFontStyle];

  TextTrackFontGenericFamilyType get currentSubtitleFontFamilyType => TextTrackFontGenericFamilyType.values[currentSubtitleFontFamily];

  final String? fFmpegExePath;
  final String? fFprobeExePath;

  final bool startFilesFromTheStart;
  final bool playNextFileAutomatically;
  final bool forceVideoTranscode;
  final bool forceAudioTranscode;
  final int videoScale;
  final bool enableHardwareAcceleration;
  final int webVideoQuality;

  final int currentSubtitleFgColor;
  final int currentSubtitleBgColor;
  final int currentSubtitleFontScale;
  final int currentSubtitleFontStyle;
  final int currentSubtitleFontFamily;
  final double subtitleDelayInSeconds;
  final bool loadFirstSubtitleFoundAutomatically;

  ServerAppSettings({
    required this.fFmpegExePath,
    required this.fFprobeExePath,
    required this.startFilesFromTheStart,
    required this.playNextFileAutomatically,
    required this.forceVideoTranscode,
    required this.forceAudioTranscode,
    required this.videoScale,
    required this.enableHardwareAcceleration,
    required this.webVideoQuality,
    required this.currentSubtitleFgColor,
    required this.currentSubtitleBgColor,
    required this.currentSubtitleFontScale,
    required this.currentSubtitleFontStyle,
    required this.currentSubtitleFontFamily,
    required this.subtitleDelayInSeconds,
    required this.loadFirstSubtitleFoundAutomatically,
  });

  factory ServerAppSettings.fromJson(Map<String, dynamic> json) => _$ServerAppSettingsFromJson(json);

  Map<String, dynamic> toJson() => _$ServerAppSettingsToJson(this);
}
