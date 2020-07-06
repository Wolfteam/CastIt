part of 'server_ws_bloc.dart';

@freezed
abstract class ServerWsEvent implements _$ServerWsEvent {
  // factory ServerMsgEvent.init() = ServerInitEvent;
  factory ServerWsEvent.connectToWs() = ServerConnectToWsEvent;
  factory ServerWsEvent.disconnectedFromWs() = ServerDisconnectedFromWsEvent;
  factory ServerWsEvent.disconnectFromWs() = ServerDisconnectFromWsEvent;
  factory ServerWsEvent.updateUrlAndConnectToWs({
    @required String castItUrl,
  }) = ServerUpdateUrlAndConnectToWsEvent;
  factory ServerWsEvent.showMsg({
    @required String msg,
  }) = ShowMsgEvent;

  const ServerWsEvent._();
}
