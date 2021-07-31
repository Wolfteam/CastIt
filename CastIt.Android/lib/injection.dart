import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:castit/domain/services/device_info_service.dart';
import 'package:castit/domain/services/locale_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:castit/domain/services/telemetry_service.dart';
import 'package:castit/infrastructure/castit_hub_client_service.dart';
import 'package:castit/infrastructure/device_info_service.dart';
import 'package:castit/infrastructure/locale_service.dart';
import 'package:castit/infrastructure/logging_service.dart';
import 'package:castit/infrastructure/settings_service.dart';
import 'package:castit/infrastructure/telemetry/telemetry_service.dart';
import 'package:get_it/get_it.dart';

final GetIt getIt = GetIt.instance;

Future<void> initInjection() async {
  final deviceInfoService = DeviceInfoServiceImpl();
  getIt.registerSingleton<DeviceInfoService>(deviceInfoService);
  await deviceInfoService.init();

  final telemetryService = TelemetryServiceImpl(deviceInfoService);
  getIt.registerSingleton<TelemetryService>(telemetryService);
  await telemetryService.initTelemetry();

  final loggingService = LoggingServiceImpl(getIt<TelemetryService>(), deviceInfoService);

  getIt.registerSingleton<LoggingService>(loggingService);
  final settingsService = SettingsServiceImpl(loggingService);
  await settingsService.init();
  getIt.registerSingleton<SettingsService>(settingsService);

  getIt.registerSingleton<LocaleService>(LocaleServiceImpl(getIt<SettingsService>()));

  getIt.registerSingleton<CastItHubClientService>(CastItHubClientServiceImpl(loggingService, settingsService));
}
