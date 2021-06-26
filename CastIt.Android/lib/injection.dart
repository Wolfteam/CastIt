import 'package:get_it/get_it.dart';

import 'services/logging_service.dart';
import 'services/settings_service.dart';

final GetIt getIt = GetIt.instance;

void initInjection() {
  getIt.registerSingleton<LoggingService>(LoggingServiceImpl());
  getIt.registerSingleton<SettingsService>(
    SettingsServiceImpl(getIt<LoggingService>()),
  );
}
