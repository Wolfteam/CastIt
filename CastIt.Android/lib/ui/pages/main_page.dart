import 'package:castit/bloc/main/main_bloc.dart';
import 'package:castit/bloc/playlist/playlist_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/playlists/playlists_bloc.dart';
import 'play_page.dart';
import 'playlists_page.dart';
import 'settings_page.dart';

class MainPage extends StatefulWidget {
  @override
  _MainPageState createState() => _MainPageState();
}

class _MainPageState extends State<MainPage>
    with SingleTickerProviderStateMixin {
  bool _didChangeDependencies = false;
  TabController _tabController;
  int _index = 0;
  final _pages = [PlayPage(), PlayListsPage(), SettingsPage()];
  @override
  void initState() {
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
    _didChangeDependencies = true;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // appBar: AppBar(
      //   title: Text("CastIt"),
      // ),
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
        showUnselectedLabels: true,
        items: _buildBottomNavBars(),
        type: BottomNavigationBarType.fixed,
        onTap: _changeCurrentTab,
      ),
    );
  }

  List<BottomNavigationBarItem> _buildBottomNavBars() {
    return [
      BottomNavigationBarItem(
        title: Text("Playing"),
        icon: Icon(Icons.play_arrow),
      ),
      BottomNavigationBarItem(
        title: Text("PlayLists"),
        icon: Icon(Icons.playlist_play),
      ),
      BottomNavigationBarItem(
        title: Text("Settings"),
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
