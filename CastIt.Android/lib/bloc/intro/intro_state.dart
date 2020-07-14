part of 'intro_bloc.dart';

@freezed
abstract class IntroState implements _$IntroState {
  factory IntroState.loading() = IntroInitialState;
  factory IntroState.loaded({
    @required String currentCastItUrl,
    @required AppLanguageType currentLang,
    @Default(false) bool urlWasSet,
    @Default(0) int page,
  }) = IntroLoadedState;
  const IntroState._();
}
