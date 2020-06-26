import 'package:dio/dio.dart';
import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';

import 'services/api/castit_api.dart';
import 'services/castit_service.dart';

final GetIt getIt = GetIt.instance;

@injectableInit
void initInjection() {
  registerApiClient();
}

//TODO: FIGURE OUT HOW TO CHANGE THIS URL
void registerApiClient({String baseUrl = 'http://192.168.1.101:9696/api'}) {
  final dio = Dio();
  dio.options.connectTimeout = 5000;

  final client = CastItApi(dio, baseUrl: baseUrl);
  getIt.registerSingleton(client);
  getIt.registerSingleton<CastItService>(CastItServiceImpl(api: client));
}
