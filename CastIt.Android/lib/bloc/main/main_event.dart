part of 'main_bloc.dart';

@freezed
class MainEvent with _$MainEvent {
  factory MainEvent.init() = MainInitEvent;

  factory MainEvent.themeChanged({
    required AppThemeType theme,
  }) = MainThemeChangedEvent;

  factory MainEvent.accentColorChanged({
    required AppAccentColorType accentColor,
  }) = MainAccentColorChangedEvent;

  factory MainEvent.goToTab({required int index}) = MainGoToTabEvent;

  factory MainEvent.introCompleted() = MainIntroCompletedEvent;
  const MainEvent._();
}
