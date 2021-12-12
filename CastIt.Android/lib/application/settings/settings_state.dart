part of 'settings_bloc.dart';

@freezed
class SettingsState with _$SettingsState {
  factory SettingsState.loading() = _LoadingState;

  factory SettingsState.loaded({
    required AppThemeType appTheme,
    required bool useDarkAmoled,
    required AppAccentColorType accentColor,
    required AppLanguageType appLanguage,
    required bool isConnected,
    required String castItUrl,
    required bool isCastItUrlValid,
    required String fFmpegExePath,
    required String fFprobeExePath,
    required VideoScaleType videoScale,
    required WebVideoQualityType webVideoQuality,
    required bool playFromTheStart,
    required bool playNextFileAutomatically,
    required bool forceVideoTranscode,
    required bool forceAudioTranscode,
    required bool enableHwAccel,
    required String appName,
    required String appVersion,
    required bool loadFirstSubtitleFoundAutomatically,
    required SubtitleFgColorType currentSubtitleFgColor,
    required SubtitleBgColorType currentSubtitleBgColor,
    required SubtitleFontScaleType currentSubtitleFontScale,
    required double subtitleDelayInSeconds,
    required TextTrackFontGenericFamilyType currentSubtitleFontFamily,
    required TextTrackFontStyleType currentSubtitleFontStyle,
  }) = _LoadedState;
}
