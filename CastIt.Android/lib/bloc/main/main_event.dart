part of 'main_bloc.dart';

@freezed
abstract class MainEvent implements _$MainEvent {
  factory MainEvent.init() = MainInitEvent;

  factory MainEvent.themeChanged({
    @required AppThemeType theme,
  }) = MainThemeChangedEvent;

  factory MainEvent.accentColorChanged({
    @required AppAccentColorType accentColor,
  }) = MainAccentColorChangedEvent;

  factory MainEvent.goToTab({@required int index}) = MainGoToTabEvent;
  const MainEvent._();
}
