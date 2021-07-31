part of 'server_ws_bloc.dart';

@freezed
class ServerWsEvent with _$ServerWsEvent {
  factory ServerWsEvent.connectToWs() = _Connect;

  factory ServerWsEvent.disconnectedFromWs() = _Disconnected;

  factory ServerWsEvent.disconnectFromWs() = _Disconnect;

  factory ServerWsEvent.updateUrlAndConnectToWs({
    required String castItUrl,
  }) = _UpdateUrlAndConnect;

  factory ServerWsEvent.showMsg({
    required AppMessageType type,
  }) = _ShowMessage;
}
