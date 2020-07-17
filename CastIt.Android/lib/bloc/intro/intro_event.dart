part of 'intro_bloc.dart';

@freezed
abstract class IntroEvent implements _$IntroEvent {
  factory IntroEvent.load() = IntroLoadEvent;
  factory IntroEvent.changePage({
    @required int newPage,
  }) = IntroChangePageEvent;
  factory IntroEvent.urlWasSet({
    @required String url,
  }) = IntroUrlSetEvent;
  factory IntroEvent.languageChanged({
    @required AppLanguageType newLang,
  }) = IntroLanguageChangedEvent;

  const IntroEvent._();
}
