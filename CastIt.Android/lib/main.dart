import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'bloc/playlist/playlist_bloc.dart';
import 'bloc/playlists/playlists_bloc.dart';
import 'bloc/settings/settings_bloc.dart';
import 'injection.dart';
import 'services/castit_service.dart';
import 'ui/pages/main_page.dart';

void main() {
  initInjection();
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(
          create: (ctx) {
            final castitService = getIt<CastItService>();
            return PlaylistsBloc(castitService);
          },
        ),
        BlocProvider(
          create: (ctx) {
            final castitService = getIt<CastItService>();
            return PlaylistBloc(castitService);
          },
        ),
        BlocProvider(
          create: (ctx) {
            return SettingsBloc();
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
      ),
    );
  }
}
