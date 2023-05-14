part of 'intro_bloc.dart';

@freezed
class IntroEvent with _$IntroEvent {
  factory IntroEvent.load() = _Load;

  factory IntroEvent.changePage({
    required int newPage,
  }) = _ChangePage;

  factory IntroEvent.urlWasSet({
    required String url,
  }) = _UrlSet;

  factory IntroEvent.languageChanged({
    required AppLanguageType newLang,
  }) = _LanguageChanged;
}
