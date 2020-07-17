part of 'main_bloc.dart';

@freezed
abstract class MainState implements _$MainState {
  factory MainState.loading() = MainLoadingState;
  factory MainState.loaded({
    @required String appTitle,
    @required ThemeData theme,
    @required bool initialized,
    @required bool firstInstall,
    @Default(0) int currentSelectedTab,
  }) = MainLoadedState;
  const MainState._();
}
