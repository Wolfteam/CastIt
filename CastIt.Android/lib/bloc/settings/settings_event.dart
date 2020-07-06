part of 'settings_bloc.dart';

@freezed
abstract class SettingsEvent implements _$SettingsEvent {
  factory SettingsEvent.load() = SettingsLoadEvent;

  factory SettingsEvent.connected({
    @required AppSettingsResponseDto settings,
  }) = SettingsConnectedEvent;

  factory SettingsEvent.disconnected() = SettingsDisconnectedEvent;

  factory SettingsEvent.themeChanged({
    @required AppThemeType theme,
  }) = SettingsThemeChangedEvent;

  factory SettingsEvent.castItUrlChanged({
    @required String castItUrl,
  }) = _SettingsCastItUrlChangedEvent;

  factory SettingsEvent.accentColorChanged({
    @required AppAccentColorType accentColor,
  }) = SettingsAccentColorChangedEvent;

  factory SettingsEvent.languageChanged({
    @required AppLanguageType lang,
  }) = SettingsLanguageChangedEvent;

  const SettingsEvent._();
}
