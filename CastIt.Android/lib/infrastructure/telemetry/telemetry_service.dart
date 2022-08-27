import 'package:castit/domain/services/device_info_service.dart';
import 'package:castit/domain/services/telemetry_service.dart';
import 'package:castit/infrastructure/telemetry/flutter_appcenter_bundle.dart';
import 'package:castit/secrets.dart';

class TelemetryServiceImpl implements TelemetryService {
  final DeviceInfoService _deviceInfoService;

  TelemetryServiceImpl(this._deviceInfoService);

  //Only call this function from the main.dart
  @override
  Future<void> initTelemetry() async {
    await AppCenter.startAsync(appSecretAndroid: Secrets.appCenterKey, appSecretIOS: '');
  }

  @override
  Future<void> trackEventAsync(String name, [Map<String, String>? properties]) {
    properties ??= {};
    properties.addAll(_deviceInfoService.deviceInfo);
    return AppCenter.trackEventAsync(name, properties);
  }
}
