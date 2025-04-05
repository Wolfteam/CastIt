part of 'settings_bloc.dart';

@freezed
sealed class SettingsEvent with _$SettingsEvent {
  const factory SettingsEvent.load() = SettingsEventLoad;

  const factory SettingsEvent.connected({required ServerAppSettings settings}) = SettingsEventConnected;

  const factory SettingsEvent.disconnected() = SettingsEventDisconnected;

  const factory SettingsEvent.themeChanged({required AppThemeType theme}) = SettingsEventThemeChanged;

  const factory SettingsEvent.castItUrlChanged({required String castItUrl}) = SettingsEventCastItUrlChanged;

  const factory SettingsEvent.accentColorChanged({required AppAccentColorType accentColor}) = SettingsEventAccentColorChanged;

  const factory SettingsEvent.languageChanged({required AppLanguageType lang}) = SettingsEventLanguageChanged;
}
