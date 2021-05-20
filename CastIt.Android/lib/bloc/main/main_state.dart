part of 'main_bloc.dart';

@freezed
class MainState with _$MainState {
  factory MainState.loading() = MainLoadingState;
  factory MainState.loaded({
    required String appTitle,
    required ThemeData theme,
    required bool initialized,
    required bool firstInstall,
    @Default(0) int currentSelectedTab,
  }) = MainLoadedState;
  const MainState._();
}
