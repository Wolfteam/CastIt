part of 'settings_bloc.dart';

@freezed
class SettingsState with _$SettingsState {
  factory SettingsState.loading() = SettingsLoadingState;
  factory SettingsState.loaded({
    required AppThemeType appTheme,
    required bool useDarkAmoled,
    required AppAccentColorType accentColor,
    required AppLanguageType appLanguage,
    required bool isConnected,
    required String castItUrl,
    required bool isCastItUrlValid,
    required VideoScaleType videoScale,
    required bool playFromTheStart,
    required bool playNextFileAutomatically,
    required bool forceVideoTranscode,
    required bool forceAudioTranscode,
    required bool enableHwAccel,
    required String appName,
    required String appVersion,
  }) = SettingsLoadedState;
  const SettingsState._();
}
