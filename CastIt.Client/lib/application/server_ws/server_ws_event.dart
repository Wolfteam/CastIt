part of 'server_ws_bloc.dart';

@freezed
sealed class ServerWsEvent with _$ServerWsEvent {
  const factory ServerWsEvent.connectToWs() = ServerWsEventConnect;

  const factory ServerWsEvent.disconnectedFromWs() = ServerWsEventDisconnected;

  const factory ServerWsEvent.disconnectFromWs() = ServerWsEventDisconnect;

  const factory ServerWsEvent.updateUrlAndConnectToWs({required String castItUrl}) = ServerWsEventUpdateUrlAndConnect;

  const factory ServerWsEvent.showMsg({required AppMessageType type}) = ServerWsEventShowMessage;
}
