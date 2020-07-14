import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'bloc/intro/intro_bloc.dart';
import 'bloc/main/main_bloc.dart';
import 'bloc/play/play_bloc.dart';
import 'bloc/played_file_options/played_file_options_bloc.dart';
import 'bloc/playlist/playlist_bloc.dart';
import 'bloc/playlist_rename/playlist_rename_bloc.dart';
import 'bloc/playlists/playlists_bloc.dart';
import 'bloc/server_ws/server_ws_bloc.dart';
import 'bloc/settings/settings_bloc.dart';
import 'generated/i18n.dart';
import 'injection.dart';
import 'logger.dart';
import 'services/logging_service.dart';
import 'services/settings_service.dart';
import 'ui/pages/intro_page.dart';
import 'ui/pages/main_page.dart';

Future main() async {
  await setupLogging();
  initInjection();
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  runApp(MyApp());
}

class MyApp extends StatefulWidget {
  @override
  _MyAppState createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  final GeneratedLocalizationsDelegate i18n = I18n.delegate;

  @override
  void initState() {
    super.initState();
    I18n.onLocaleChanged = _onLocaleChange;
  }

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            return MainBloc(logger, settings)..add(MainEvent.init());
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
            final serverWsBloc = ctx.bloc<ServerWsBloc>();
            return PlayListBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final serverWsBloc = ctx.bloc<ServerWsBloc>();
            return PlayListsBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final serverWsBloc = ctx.bloc<ServerWsBloc>();
            return PlayBloc(serverWsBloc);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            final serverWsBloc = ctx.bloc<ServerWsBloc>();
            return SettingsBloc(logger, settings, serverWsBloc);
          },
        ),
        BlocProvider(create: (ctx) {
          final serverWsBloc = ctx.bloc<ServerWsBloc>();
          return PlayedFileOptionsBloc(serverWsBloc);
        }),
        BlocProvider(create: (ctx) => PlayListRenameBloc()),
        BlocProvider(create: (ctx) {
          final settings = getIt<SettingsService>();
          final settingsBloc = ctx.bloc<SettingsBloc>();
          return IntroBloc(settings, settingsBloc);
        }),
      ],
      child: BlocBuilder<MainBloc, MainState>(
        builder: (ctx, state) => _buildApp(state),
      ),
    );
  }

  Widget _buildApp(MainState state) {
    final delegates = <LocalizationsDelegate>[
      // A class which loads the translations from JSON files
      i18n,
      // Built-in localization of basic text for Material widgets
      GlobalMaterialLocalizations.delegate,
      // Built-in localization for text direction LTR/RTL
      GlobalWidgetsLocalizations.delegate,
      // Built-in localization of basic text for Cupertino widgets
      GlobalCupertinoLocalizations.delegate,
    ];
    return state.map<Widget>(
      loading: (_) {
        return const CircularProgressIndicator();
      },
      loaded: (s) {
        return MaterialApp(
          title: s.appTitle,
          theme: s.theme,
          home: s.firstInstall ? IntroPage() : MainPage(),
          localizationsDelegates: delegates,
          supportedLocales: i18n.supportedLocales,
          localeResolutionCallback: i18n.resolution(
            fallback: i18n.supportedLocales.first,
          ),
        );
      },
    );
  }

  void _onLocaleChange(Locale locale) {
    setState(() {
      I18n.locale = locale;
    });
  }
}
