part of 'intro_bloc.dart';

@freezed
sealed class IntroEvent with _$IntroEvent {
  const factory IntroEvent.load() = IntroEventLoad;

  const factory IntroEvent.changePage({required int newPage}) = IntroEventChangePage;

  const factory IntroEvent.urlWasSet({required String url}) = IntroEventUrlSet;

  const factory IntroEvent.languageChanged({required AppLanguageType newLang}) = IntroEventLanguageChanged;
}
