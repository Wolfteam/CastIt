import 'package:dio/dio.dart';
import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import 'package:log_4_dart_2/log_4_dart_2.dart';

import 'services/api/castit_api.dart';
import 'services/castit_service.dart';
import 'services/logging_service.dart';
import 'services/settings_service.dart';

final GetIt getIt = GetIt.instance;

@injectableInit
void initInjection() {
  registerApiClient();
  getIt.registerSingleton(Logger());
  getIt.registerSingleton<LoggingService>(LoggingServiceImpl(getIt<Logger>()));
  getIt.registerSingleton<SettingsService>(
    SettingsServiceImpl(getIt<LoggingService>()),
  );
}

//TODO: FIGURE OUT HOW TO CHANGE THIS URL
void registerApiClient({String baseUrl = 'http://192.168.1.101:9696/api'}) {
  final dio = Dio();
  dio.options.connectTimeout = 3000;

  final client = CastItApi(dio, baseUrl: baseUrl);
  getIt.registerSingleton(client);
  getIt.registerSingleton<CastItService>(CastItServiceImpl(api: client));
}
