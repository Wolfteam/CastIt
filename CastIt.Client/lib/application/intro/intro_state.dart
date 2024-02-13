part of 'intro_bloc.dart';

@freezed
class IntroState with _$IntroState {
  const factory IntroState.loading() = _LoadingState;

  const factory IntroState.loaded({
    required String currentCastItUrl,
    required AppLanguageType currentLang,
    @Default(false) bool urlWasSet,
    @Default(0) int page,
  }) = _LoadedState;
}
