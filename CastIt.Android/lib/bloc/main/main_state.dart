part of 'main_bloc.dart';

@freezed
abstract class MainState implements _$MainState {
  factory MainState.loading() = MainLoadingState;
  factory MainState.loaded() = MainLoadedState;
  const MainState._();
}
