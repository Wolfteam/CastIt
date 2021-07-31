part of 'main_bloc.dart';

@freezed
class MainEvent with _$MainEvent {
  factory MainEvent.init() = _Init;

  factory MainEvent.themeChanged({
    required AppThemeType theme,
  }) = _ThemeChanged;

  factory MainEvent.accentColorChanged({
    required AppAccentColorType accentColor,
  }) = _AccentColorChanged;

  factory MainEvent.goToTab({required int index}) = _GoToTab;

  factory MainEvent.introCompleted() = _IntroCompleted;

  factory MainEvent.languageChanged() = _LanguageChanged;
}
