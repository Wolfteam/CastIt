import 'package:castit/bloc/playlist/playlist_bloc.dart';
import 'package:castit/bloc/playlists/playlists_bloc.dart';
import 'package:castit/common/enums/app_message_type.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/main/main_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../bloc/settings/settings_bloc.dart';
import '../../common/extensions/i18n_extensions.dart';
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
  late TabController _tabController;
  int _index = 0;
  bool _canShowConnectionModal = true;
  final _pages = [PlayPage(), PlayListsPage(), SettingsPage()];

  @override
  void initState() {
    WidgetsBinding.instance!.addObserver(this);
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
    //If we don't do this, the PlayListsBloc and SettingsBloc constructors won't be called
    //ending in the fact that we won't listen to the hub events
    context.read<SettingsBloc>().add(SettingsEvent.load());
    context.read<PlayListsBloc>().listenHubEvents();
    context.read<PlayListBloc>().listenHubEvents();
    context.read<SettingsBloc>().listenHubEvents();
    context.read<ServerWsBloc>().add(ServerWsEvent.connectToWs());
    _didChangeDependencies = true;
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    debugPrint('State = $state');
    if (state == AppLifecycleState.inactive) {
      _canShowConnectionModal = false;
      context.read<ServerWsBloc>().add(ServerWsEvent.disconnectFromWs());
    } else if (state == AppLifecycleState.resumed) {
      _canShowConnectionModal = true;
      context.read<ServerWsBloc>().add(ServerWsEvent.connectToWs());
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
              state2.map(
                loaded: (s) async {
                  if (s.msgToShow != null) {
                    _showServerMsg(ctx2, s.msgToShow!);
                  }
                  await _showConnectionDialog(s.isConnectedToWs!, s.castItUrl);
                },
                loading: (s) {
                  if (_isShowingConnectionModal) {
                    Navigator.pop(context);
                  }
                },
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
        onTap: (newIndex) => context.read<MainBloc>().add(MainEvent.goToTab(index: newIndex)),
      ),
    );
  }

  @override
  void dispose() {
    WidgetsBinding.instance!.removeObserver(this);
    super.dispose();
  }

  List<BottomNavigationBarItem> _buildBottomNavBars() {
    final i18n = I18n.of(context)!;
    return [
      BottomNavigationBarItem(
        label: i18n.playing,
        icon: const Icon(Icons.play_arrow),
      ),
      BottomNavigationBarItem(
        label: i18n.playlists,
        icon: const Icon(Icons.playlist_play),
      ),
      BottomNavigationBarItem(
        label: i18n.settings,
        icon: const Icon(Icons.settings),
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
    debugPrint('IsConnected = $isConnected - ShowingModal = $_isShowingConnectionModal');
    if (!isConnected && !_isShowingConnectionModal && _canShowConnectionModal) {
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

  void _showServerMsg(BuildContext ctx, AppMessageType msg) {
    final theme = Theme.of(ctx);
    final color = theme.accentColor.withOpacity(0.8);
    final s = I18n.of(context);

    final snackBar = SnackBar(
      behavior: SnackBarBehavior.floating,
      backgroundColor: color,
      content: Row(
        children: <Widget>[
          const Icon(Icons.info_outline, color: Colors.white),
          Container(
            margin: const EdgeInsets.only(left: 10),
            child: Text(
              s!.translateAppMsgType(msg),
              style: const TextStyle(color: Colors.white),
              overflow: TextOverflow.ellipsis,
            ),
          ),
        ],
      ),
      duration: const Duration(seconds: 3),
    );
    ScaffoldMessenger.of(context).showSnackBar(snackBar);
  }
}
