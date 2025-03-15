part of 'server_ws_bloc.dart';

@freezed
sealed class ServerWsState with _$ServerWsState {
  const factory ServerWsState.loading() = ServerWsStateLoadingState;

  const factory ServerWsState.loaded({
    required String castItUrl,
    bool? isConnectedToWs,
    int? connectionRetries,
    AppMessageType? msgToShow,
  }) = ServerWsStateLoadedState;
}
