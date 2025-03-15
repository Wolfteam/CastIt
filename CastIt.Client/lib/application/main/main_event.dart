part of 'main_bloc.dart';

@freezed
sealed class MainEvent with _$MainEvent {
  const factory MainEvent.init() = MainEventInit;

  const factory MainEvent.themeChanged({required AppThemeType theme}) = MainEventThemeChanged;

  const factory MainEvent.accentColorChanged({required AppAccentColorType accentColor}) = MainEventAccentColorChanged;

  const factory MainEvent.goToTab({required int index}) = MainEventGoToTab;

  const factory MainEvent.introCompleted() = MainEventIntroCompleted;

  const factory MainEvent.languageChanged() = MainEventLanguageChanged;
}
