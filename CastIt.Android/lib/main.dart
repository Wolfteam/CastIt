import 'package:castit/domain/services/castit_hub_client_service.dart';
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

Future<void> main() async {
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
      loading: (_) => const CircularProgressIndicator(),
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
