part of 'server_ws_bloc.dart';

@freezed
class ServerWsState with _$ServerWsState {
  factory ServerWsState.loading() = _LoadingState;

  factory ServerWsState.loaded({
    required String castItUrl,
    bool? isConnectedToWs,
    int? connectionRetries,
    AppMessageType? msgToShow,
  }) = _LoadedState;
}
