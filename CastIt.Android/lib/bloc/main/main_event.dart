part of 'main_bloc.dart';

@freezed
abstract class MainEvent implements _$MainEvent {
  factory MainEvent.init() = MainInitEvent;
  factory MainEvent.connectToWs() = MainConnectToWsEvent;
  factory MainEvent.disconnectFromWs() = MainDisconnectFromWsEvent;
  factory MainEvent.themeChanged({
    @required AppThemeType theme,
  }) = MainThemeChangedEvent;

  factory MainEvent.accentColorChanged({
    @required AppAccentColorType accentColor,
  }) = MainAccentColorChangedEvent;
  const MainEvent._();
}
