part of 'devices_bloc.dart';

@freezed
sealed class DevicesEvent with _$DevicesEvent {
  const factory DevicesEvent.fetch() = DevicesEventFetch;

  const factory DevicesEvent.loaded({required List<Receiver> devices}) = DevicesEventLoaded;

  const factory DevicesEvent.deviceSet({required Receiver device}) = DevicesEventDeviceSet;

  const factory DevicesEvent.disconnected() = DevicesEventDisconnected;

  const factory DevicesEvent.connect({String? id}) = DevicesEventConnect;

  const factory DevicesEvent.refresh() = DevicesEventRefresh;

  const factory DevicesEvent.addEndpoint({required String name, required String host, required int port}) = DevicesEventAddEndpoint;

  const factory DevicesEvent.editEndpoint({
    required int id,
    required String name,
    required String host,
    required int port,
  }) = DevicesEventEditEndpoint;

  const factory DevicesEvent.deleteEndpoint({required int id}) = DevicesEventDeleteEndpoint;
}
