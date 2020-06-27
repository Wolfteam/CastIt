import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'bloc/main/main_bloc.dart';
import 'bloc/playlist/playlist_bloc.dart';
import 'bloc/playlists/playlists_bloc.dart';
import 'bloc/settings/settings_bloc.dart';
import 'generated/i18n.dart';
import 'injection.dart';
import 'logger.dart';
import 'services/castit_service.dart';
import 'services/logging_service.dart';
import 'services/settings_service.dart';
import 'ui/pages/main_page.dart';

Future main() async {
  await setupLogging();
  initInjection();
  runApp(MyApp());
}

class MyApp extends StatefulWidget {
  @override
  _MyAppState createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  final i18n = I18n.delegate;

  @override
  void initState() {
    super.initState();
    I18n.onLocaleChanged = _onLocaleChange;
  }

  @override
  Widget build(BuildContext context) {
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
    return MultiBlocProvider(
      providers: [
        BlocProvider(
          create: (ctx) {
            final castitService = getIt<CastItService>();
            return PlayListsBloc(castitService);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castitService = getIt<CastItService>();
            return PlayListBloc(castitService);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            return SettingsBloc(logger, settings)..add(SettingsEvent.load());
          },
        ),
        BlocProvider(
          create: (ctx) {
            final logger = getIt<LoggingService>();
            final settings = getIt<SettingsService>();
            return MainBloc(logger, settings)..add(MainEvent.init());
          },
        )
      ],
      child: MaterialApp(
        title: 'CastIt',
        theme: ThemeData(
          primarySwatch: Colors.blue,
          visualDensity: VisualDensity.adaptivePlatformDensity,
        ),
        home: MainPage(),
        localizationsDelegates: delegates,
        supportedLocales: i18n.supportedLocales,
        localeResolutionCallback: i18n.resolution(
          fallback: i18n.supportedLocales.first,
        ),
      ),
    );
  }

  void _onLocaleChange(Locale locale) {
    setState(() {
      I18n.locale = locale;
    });
  }
}
