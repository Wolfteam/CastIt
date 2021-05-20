part of 'intro_bloc.dart';

@freezed
class IntroEvent with _$IntroEvent {
  factory IntroEvent.load() = IntroLoadEvent;
  factory IntroEvent.changePage({
    required int newPage,
  }) = IntroChangePageEvent;
  factory IntroEvent.urlWasSet({
    required String url,
  }) = IntroUrlSetEvent;
  factory IntroEvent.languageChanged({
    required AppLanguageType newLang,
  }) = IntroLanguageChangedEvent;

  const IntroEvent._();
}
