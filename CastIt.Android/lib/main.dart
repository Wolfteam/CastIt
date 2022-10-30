import 'dart:io';

import 'package:castit/application/bloc.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:castit/domain/services/device_info_service.dart';
import 'package:castit/domain/services/locale_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:castit/injection.dart';
import 'package:castit/presentation/app_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:window_size/window_size.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initInjection();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  if (Platform.isWindows || Platform.isLinux || Platform.isMacOS) {
    setWindowMinSize(const Size(400, 700));
    setWindowMaxSize(Size.infinite);
  }
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            final deviceInfo = getIt<DeviceInfoService>();
            final localeService = getIt<LocaleService>();
            return MainBloc(logger, settings, deviceInfo, localeService)..add(MainEvent.init());
          },
        ),
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            final castItHub = getIt<CastItHubClientService>();
            return ServerWsBloc(logger, settings, castItHub);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castItHub = getIt<CastItHubClientService>();
            return PlayListsBloc(castItHub);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castItHub = getIt<CastItHubClientService>();
            return PlayBloc(castItHub);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final settings = getIt<SettingsService>();
            final castItHub = getIt<CastItHubClientService>();
            return SettingsBloc(settings, ctx.read<MainBloc>(), castItHub);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castItHub = getIt<CastItHubClientService>();
            return PlayedFileOptionsBloc(castItHub);
          },
        ),
        BlocProvider(create: (ctx) => PlayListRenameBloc()),
        BlocProvider(
          create: (ctx) {
            final settings = getIt<SettingsService>();
            final settingsBloc = ctx.read<SettingsBloc>();
            return IntroBloc(settings, settingsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castItHub = getIt<CastItHubClientService>();
            return PlayedPlayListItemBloc(castItHub);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castItHub = getIt<CastItHubClientService>();
            return PlayedFileItemBloc(castItHub);
          },
        ),
      ],
      child: const AppWidget(),
    );
  }
}
