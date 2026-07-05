import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'devices_bloc.freezed.dart';
part 'devices_event.dart';
part 'devices_state.dart';

class DevicesBloc extends Bloc<DevicesEvent, DevicesState> {
  final CastItHubClientService _castItHub;

  DevicesLoadedState get currentState => state as DevicesLoadedState;

  DevicesBloc(this._castItHub) : super(const DevicesState.loaded()) {
    on<DevicesEventFetch>((event, emit) async {
      await _castItHub.loadDevices();
    });

    on<DevicesEventLoaded>((event, emit) {
      emit(DevicesState.loaded(devices: event.devices));
    });

    on<DevicesEventDeviceSet>((event, emit) {
      final devices = currentState.devices
          .map((d) => d.copyWith(isConnected: d.id == event.device.id))
          .toList();
      emit(DevicesState.loaded(devices: devices));
    });

    on<DevicesEventDisconnected>((event, emit) {
      final devices = currentState.devices.map((d) => d.copyWith(isConnected: false)).toList();
      emit(DevicesState.loaded(devices: devices));
    });

    on<DevicesEventConnect>((event, emit) async {
      await _castItHub.connectToCastDevice(event.id);
    });

    on<DevicesEventRefresh>((event, emit) async {
      await _castItHub.refreshCastDevices();
    });

    on<DevicesEventAddEndpoint>((event, emit) async {
      await _castItHub.addCustomEndpoint(event.name, event.host, event.port);
    });

    on<DevicesEventEditEndpoint>((event, emit) async {
      await _castItHub.updateCustomEndpoint(event.id, event.name, event.host, event.port);
    });

    on<DevicesEventDeleteEndpoint>((event, emit) async {
      await _castItHub.deleteCustomEndpoint(event.id);
    });

    _castItHub.castDevicesChanged.stream.listen((d) => add(DevicesEvent.loaded(devices: d)));
    _castItHub.castDeviceSet.stream.listen((d) => add(DevicesEvent.deviceSet(device: d)));
    _castItHub.castDeviceDisconnected.stream.listen((_) => add(const DevicesEvent.disconnected()));

    //Subscriptions are set up above, so requesting the list now guarantees we receive the reply.
    add(const DevicesEvent.fetch());
  }
}
