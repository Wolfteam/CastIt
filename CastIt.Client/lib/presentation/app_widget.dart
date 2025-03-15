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
      builder:
          (ctx, state) => switch (state) {
            MainStateLoadingState() => const CircularProgressIndicator(),
            MainStateLoadedState() => MaterialApp(
              title: state.appTitle,
              theme: state.accentColor.getThemeData(state.theme),
              home: AnnotatedRegion(
                value: state.theme == AppThemeType.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
                child: state.firstInstall ? IntroPage() : MainPage(),
              ),
              //Without this, the lang won't be reloaded
              locale: Locale(state.language.code, state.language.countryCode),
              localizationsDelegates: const <LocalizationsDelegate>[
                S.delegate,
                GlobalMaterialLocalizations.delegate,
                GlobalWidgetsLocalizations.delegate,
                GlobalCupertinoLocalizations.delegate,
              ],
              supportedLocales: S.delegate.supportedLocales,
              scrollBehavior: MyCustomScrollBehavior(),
            ),
          },
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
    PointerDeviceKind.trackpad,
    PointerDeviceKind.stylus,
  };
}
