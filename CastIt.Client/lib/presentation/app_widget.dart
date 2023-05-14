import 'dart:ui';

import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/intro/intro_page.dart';
import 'package:castit/presentation/main_page.dart';
import 'package:castit/presentation/shared/extensions/app_theme_type_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

class AppWidget extends StatelessWidget {
  const AppWidget({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<MainBloc, MainState>(
      builder: (ctx, state) => state.map<Widget>(
        loading: (_) => const CircularProgressIndicator(),
        loaded: (s) {
          final delegates = <LocalizationsDelegate>[
            S.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
            GlobalCupertinoLocalizations.delegate,
          ];
          final locale = Locale(s.language.code, s.language.countryCode);
          final themeData = s.accentColor.getThemeData(s.theme);
          return MaterialApp(
            title: s.appTitle,
            theme: themeData,
            home: AnnotatedRegion(
              value: s.theme == AppThemeType.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
              child: s.firstInstall ? IntroPage() : MainPage(),
            ),
            //Without this, the lang won't be reloaded
            locale: locale,
            localizationsDelegates: delegates,
            supportedLocales: S.delegate.supportedLocales,
            scrollBehavior: MyCustomScrollBehavior(),
          );
        },
      ),
    );
  }
}

// Since 2.5 the scroll behavior changed on desktop,
// this keeps the old one working
class MyCustomScrollBehavior extends MaterialScrollBehavior {
  @override
  Set<PointerDeviceKind> get dragDevices => {
        PointerDeviceKind.touch,
        PointerDeviceKind.mouse,
      };
}
