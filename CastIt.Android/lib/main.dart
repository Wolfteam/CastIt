import 'package:castit/domain/services/device_info_service.dart';
import 'package:castit/domain/services/locale_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/injection.dart';
import 'package:castit/presentation/shared/extensions/app_theme_type_extensions.dart';
import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'application/bloc.dart';
import 'presentation/intro/intro_page.dart';
import 'presentation/main/main_page.dart';

Future main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initInjection();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
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
            return ServerWsBloc(logger, settings);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final serverWsBloc = ctx.read<ServerWsBloc>();
            return PlayListBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final serverWsBloc = ctx.read<ServerWsBloc>();
            return PlayListsBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final serverWsBloc = ctx.read<ServerWsBloc>();
            return PlayBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final settings = getIt<SettingsService>();
            final serverWsBloc = ctx.read<ServerWsBloc>();
            return SettingsBloc(settings, serverWsBloc, ctx.read<MainBloc>());
          },
        ),
        BlocProvider(create: (ctx) {
          final serverWsBloc = ctx.read<ServerWsBloc>();
          return PlayedFileOptionsBloc(serverWsBloc);
        }),
        BlocProvider(create: (ctx) => PlayListRenameBloc()),
        BlocProvider(create: (ctx) {
          final settings = getIt<SettingsService>();
          final settingsBloc = ctx.read<SettingsBloc>();
          return IntroBloc(settings, settingsBloc);
        }),
        BlocProvider(create: (ctx) {
          final serverWsBloc = ctx.read<ServerWsBloc>();
          return PlayedPlayListItemBloc(serverWsBloc);
        }),
        BlocProvider(create: (ctx) {
          final serverWsBloc = ctx.read<ServerWsBloc>();
          return PlayedFileItemBloc(serverWsBloc);
        }),
      ],
      child: BlocBuilder<MainBloc, MainState>(
        builder: (ctx, state) => _buildApp(state),
      ),
    );
  }

  Widget _buildApp(MainState state) {
    final delegates = <LocalizationsDelegate>[
      S.delegate,
      GlobalMaterialLocalizations.delegate,
      GlobalWidgetsLocalizations.delegate,
      GlobalCupertinoLocalizations.delegate,
    ];
    return state.map<Widget>(
      loading: (_) {
        return const CircularProgressIndicator();
      },
      loaded: (s) {
        final locale = Locale(s.language.code, s.language.countryCode);
        final themeData = s.accentColor.getThemeData(s.theme);
        return MaterialApp(
          title: s.appTitle,
          theme: themeData,
          home: s.firstInstall ? IntroPage() : MainPage(),
          //Without this, the lang won't be reloaded
          locale: locale,
          localizationsDelegates: delegates,
          supportedLocales: S.delegate.supportedLocales,
        );
      },
    );
  }
}
