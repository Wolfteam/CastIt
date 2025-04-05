part of 'main_bloc.dart';

@freezed
sealed class MainState with _$MainState {
  const factory MainState.loading() = MainStateLoadingState;

  const factory MainState.loaded({
    required String appTitle,
    required AppThemeType theme,
    required AppAccentColorType accentColor,
    required bool initialized,
    required bool firstInstall,
    required LanguageModel language,
    @Default(0) int currentSelectedTab,
  }) = MainStateLoadedState;
}
