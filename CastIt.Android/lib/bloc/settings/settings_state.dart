part of 'settings_bloc.dart';

@freezed
abstract class SettingsState implements _$SettingsState {
  factory SettingsState.loading() = SettingsLoadingState;
  factory SettingsState.loaded({
    @required AppThemeType appTheme,
    @required bool useDarkAmoled,
    @required AppAccentColorType accentColor,
    @required AppLanguageType appLanguage,
    @required String castItUrl,
    @required String appName,
    @required String appVersion,
  }) = SettingsLoadedState;
  const SettingsState._();
}
