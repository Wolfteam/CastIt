part of 'main_bloc.dart';

@freezed
class MainState with _$MainState {
  factory MainState.loading() = _LoadingState;

  factory MainState.loaded({
    required String appTitle,
    required AppThemeType theme,
    required AppAccentColorType accentColor,
    required bool initialized,
    required bool firstInstall,
    required LanguageModel language,
    @Default(0) int currentSelectedTab,
  }) = _LoadedState;
}
