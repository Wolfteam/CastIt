import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/main/main_bloc.dart';
import '../../bloc/playlists/playlists_bloc.dart';
import '../../bloc/settings/settings_bloc.dart';
import '../../generated/i18n.dart';
import 'play_page.dart';
import 'playlists_page.dart';
import 'settings_page.dart';

class MainPage extends StatefulWidget {
  @override
  _MainPageState createState() => _MainPageState();
}

class _MainPageState extends State<MainPage> with SingleTickerProviderStateMixin, WidgetsBindingObserver {
  bool _didChangeDependencies = false;
  TabController _tabController;
  int _index = 0;
  final _pages = [PlayPage(), PlayListsPage(), SettingsPage()];
  @override
  void initState() {
    WidgetsBinding.instance.addObserver(this);
    _tabController = TabController(
      initialIndex: _index,
      length: _pages.length,
      vsync: this,
    );
    super.initState();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_didChangeDependencies) return;
    context.bloc<PlayListsBloc>().add(PlayListsEvent.load());
    context.bloc<SettingsBloc>().add(SettingsEvent.load());
    context.bloc<MainBloc>().add(MainEvent.connectToWs());
    _didChangeDependencies = true;
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    print('State = $state');
    if (state == AppLifecycleState.inactive) {
      context.bloc<MainBloc>().add(MainEvent.disconnectFromWs());
    } else if (state == AppLifecycleState.resumed) {
      context.bloc<MainBloc>().add(MainEvent.connectToWs());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: BlocBuilder<MainBloc, MainState>(
          builder: (ctx, state) => TabBarView(
            controller: _tabController,
            physics: const NeverScrollableScrollPhysics(),
            children: _pages,
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _index,
        showUnselectedLabels: true,
        items: _buildBottomNavBars(),
        type: BottomNavigationBarType.fixed,
        onTap: _changeCurrentTab,
      ),
    );
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  List<BottomNavigationBarItem> _buildBottomNavBars() {
    final i18n = I18n.of(context);
    return [
      BottomNavigationBarItem(
        title: Text(
          i18n.playing,
          textAlign: TextAlign.center,
          overflow: TextOverflow.ellipsis,
        ),
        icon: Icon(Icons.play_arrow),
      ),
      BottomNavigationBarItem(
        title: Text(
          i18n.playlists,
          textAlign: TextAlign.center,
          overflow: TextOverflow.ellipsis,
        ),
        icon: Icon(Icons.playlist_play),
      ),
      BottomNavigationBarItem(
        title: Text(
          i18n.settings,
          textAlign: TextAlign.center,
          overflow: TextOverflow.ellipsis,
        ),
        icon: Icon(Icons.settings),
      ),
    ];
  }

  void _changeCurrentTab(int index) {
    setState(() {
      _index = index;
      _tabController.index = index;
    });
  }
}
