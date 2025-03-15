part of 'intro_bloc.dart';

@freezed
sealed class IntroState with _$IntroState {
  const factory IntroState.loading() = IntroStateLoadingState;

  const factory IntroState.loaded({
    required String currentCastItUrl,
    required AppLanguageType currentLang,
    @Default(false) bool urlWasSet,
    @Default(0) int page,
  }) = IntroStateLoadedState;
}
