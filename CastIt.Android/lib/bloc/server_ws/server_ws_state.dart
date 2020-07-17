part of 'server_ws_bloc.dart';

@freezed
abstract class ServerWsState implements _$ServerWsState {
  const ServerWsState._();
  factory ServerWsState.loading() = ServerLoadingState;
  factory ServerWsState.loaded({
    @required String castItUrl,
    bool isConnectedToWs,
    int connectionRetries,
    String msgToShow,
  }) = ServerLoadedState;
}
