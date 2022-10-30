part of 'settings_bloc.dart';

@freezed
class SettingsEvent with _$SettingsEvent {
  factory SettingsEvent.load() = _Load;

  factory SettingsEvent.connected({
    required ServerAppSettings settings,
  }) = _Connected;

  factory SettingsEvent.disconnected() = _Disconnected;

  factory SettingsEvent.themeChanged({
    required AppThemeType theme,
  }) = _ThemeChanged;

  factory SettingsEvent.castItUrlChanged({
    required String castItUrl,
  }) = _CastItUrlChanged;

  factory SettingsEvent.accentColorChanged({
    required AppAccentColorType accentColor,
  }) = _AccentColorChanged;

  factory SettingsEvent.languageChanged({
    required AppLanguageType lang,
  }) = _LanguageChanged;
}
