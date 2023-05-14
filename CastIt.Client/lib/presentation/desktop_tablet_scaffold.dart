import 'package:castit/application/main/main_bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/play/play_page.dart';
import 'package:castit/presentation/playlists/playlists_page.dart';
import 'package:castit/presentation/settings/settings_page.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DesktopTabletScaffold extends StatefulWidget {
  const DesktopTabletScaffold({super.key});

  @override
  State<DesktopTabletScaffold> createState() => _DesktopTabletScaffoldState();
}

class _DesktopTabletScaffoldState extends State<DesktopTabletScaffold> with SingleTickerProviderStateMixin {
  int _index = 0;
  late TabController _tabController;
  bool _initialized = false;

  @override
  void initState() {
    _tabController = TabController(
      initialIndex: _index,
      length: 2,
      vsync: this,
    );

    super.initState();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_initialized) {
      return;
    }
    //This is to make sure that if a resize happens, we always start in the default index
    context.read<MainBloc>().add(MainEvent.goToTab(index: _index));
    _initialized = true;
  }

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return BlocConsumer<MainBloc, MainState>(
      listener: (ctx, state) async {
        state.maybeMap(
          loaded: (s) => _changeCurrentTab(s.currentSelectedTab),
          orElse: () {},
        );
      },
      builder: (context, state) => Scaffold(
        body: SafeArea(
          child: TabBarView(
            controller: _tabController,
            physics: const NeverScrollableScrollPhysics(),
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Expanded(child: PlayPage()),
                  Expanded(child: PlayListsPage()),
                ],
              ),
              const SettingsPage()
            ],
          ),
        ),
        bottomNavigationBar: BottomNavigationBar(
          currentIndex: _index,
          type: BottomNavigationBarType.fixed,
          showUnselectedLabels: true,
          showSelectedLabels: true,
          iconSize: 40,
          items: [
            BottomNavigationBarItem(
              label: i18n.playing,
              icon: const Icon(Icons.play_arrow),
            ),
            BottomNavigationBarItem(
              label: i18n.settings,
              icon: const Icon(Icons.settings),
            ),
          ],
          onTap: (newIndex) => context.read<MainBloc>().add(MainEvent.goToTab(index: newIndex)),
        ),
      ),
    );
  }

  void _changeCurrentTab(int index) {
    setState(() {
      _index = index;
      _tabController.index = index;
    });
  }
}
