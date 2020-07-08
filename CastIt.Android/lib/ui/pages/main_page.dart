import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/main/main_bloc.dart';
import '../../bloc/playlists/playlists_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../bloc/settings/settings_bloc.dart';
import '../../common/extensions/string_extensions.dart';
import '../../common/styles.dart';
import '../../generated/i18n.dart';
import '../widgets/modals/change_connection_bottom_sheet_dialog.dart';
import 'play_page.dart';
import 'playlists_page.dart';
import 'settings_page.dart';

class MainPage extends StatefulWidget {
  @override
  _MainPageState createState() => _MainPageState();
}

class _MainPageState extends State<MainPage> with SingleTickerProviderStateMixin, WidgetsBindingObserver {
  bool _isShowingConnectionModal = false;
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
    context.bloc<ServerWsBloc>().add(ServerWsEvent.connectToWs());
    context.bloc<PlayListsBloc>().add(PlayListsEvent.load());
    context.bloc<SettingsBloc>().add(SettingsEvent.load());
    _didChangeDependencies = true;
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    debugPrint('State = $state');
    if (state == AppLifecycleState.inactive) {
      context.bloc<ServerWsBloc>().add(ServerWsEvent.disconnectFromWs());
    } else if (state == AppLifecycleState.resumed) {
      context.bloc<ServerWsBloc>().add(ServerWsEvent.connectToWs());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: BlocConsumer<MainBloc, MainState>(
          listener: (ctx, state) async {
            if (state is MainLoadedState) {
              _changeCurrentTab(state.currentSelectedTab);
            }
          },
          builder: (ctx, state) => BlocConsumer<ServerWsBloc, ServerWsState>(
            listener: (ctx2, state2) async {
              state2.maybeMap(
                loaded: (s) async {
                  if (!s.msgToShow.isNullEmptyOrWhitespace) {
                    _showServerMsg(s.msgToShow);
                  }
                  await _showConnectionDialog(s.isConnectedToWs, s.castItUrl);
                },
                orElse: () {},
              );
            },
            builder: (ctx2, state2) => TabBarView(
              controller: _tabController,
              physics: const NeverScrollableScrollPhysics(),
              children: _pages,
            ),
          ),
        ),
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _index,
        showUnselectedLabels: true,
        items: _buildBottomNavBars(),
        type: BottomNavigationBarType.fixed,
        onTap: (newIndex) => context.bloc<MainBloc>().add(MainEvent.goToTab(index: newIndex)),
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

  Future<void> _showConnectionDialog(bool isConnected, String currentCastIt) async {
    if (!isConnected && !_isShowingConnectionModal) {
      _isShowingConnectionModal = true;
      await showModalBottomSheet(
        context: context,
        shape: Styles.modalBottomSheetShape,
        isDismissible: true,
        isScrollControlled: true,
        builder: (_) => ChangeConnectionBottomSheetDialog(
          currentUrl: currentCastIt,
        ),
      );
      _isShowingConnectionModal = false;
    } else if (isConnected && _isShowingConnectionModal) {
      Navigator.of(context).pop();
      _isShowingConnectionModal = false;
    }
  }

  void _showServerMsg(String msg) {
    final snackBar = SnackBar(content: Text(msg));
    Scaffold.of(context).showSnackBar(snackBar);
  }
}
