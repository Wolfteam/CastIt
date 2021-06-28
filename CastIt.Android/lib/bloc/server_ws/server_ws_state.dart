part of 'server_ws_bloc.dart';

@freezed
class ServerWsState with _$ServerWsState {
  const ServerWsState._();

  factory ServerWsState.loading() = ServerLoadingState;

  factory ServerWsState.loaded({
    required String castItUrl,
    bool? isConnectedToWs,
    int? connectionRetries,
    AppMessageType? msgToShow,
  }) = ServerLoadedState;
}
