import 'package:castit/application/bloc.dart';
import 'package:castit/presentation/playlist/widgets/playlist_content_disconnected.dart';
import 'package:castit/presentation/playlist/widgets/playlist_content_loaded.dart';
import 'package:castit/presentation/playlist/widgets/playlist_content_loading.dart';
import 'package:castit/presentation/playlist/widgets/playlist_content_notfound.dart';
import 'package:castit/presentation/playlist/widgets/playlist_fab.dart';
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:nil/nil.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

class PlayListPage extends StatefulWidget {
  final int id;
  final int? scrollToFileId;

  const PlayListPage({
    required this.id,
    this.scrollToFileId,
  });

  static Future<void> forDetails(int playListId, int? scrollToFileId, BuildContext context) async {
    context.read<PlayListBloc>().add(PlayListEvent.load(id: playListId, scrollToFileId: scrollToFileId));
    final route = MaterialPageRoute(builder: (_) => PlayListPage(id: playListId, scrollToFileId: scrollToFileId));
    await Navigator.of(context).push(route);
    await route.completed;
    context.read<PlayListBloc>().add(const PlayListEvent.closePage());
  }

  @override
  _PlayListPageState createState() => _PlayListPageState();
}

class _PlayListPageState extends State<PlayListPage> with SingleTickerProviderStateMixin {
  final _refreshController = RefreshController();
  final _listViewScrollController = ScrollController();
  final _itemHeight = 75.0;
  late AnimationController _hideFabAnimController;

  @override
  void initState() {
    super.initState();

    _hideFabAnimController = AnimationController(
      vsync: this,
      duration: kThemeAnimationDuration,
      value: 1, // initially visible
    );
    _listViewScrollController.addListener(_onListViewScroll);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: BlocConsumer<PlayListBloc, PlayListState>(
          listener: (ctx, state) => state.maybeMap(
            loaded: (state) {
              _refreshController.refreshCompleted();
              if (state.scrollToFileId != null) {
                final index = state.files.indexWhere((el) => el.id == state.scrollToFileId!);
                if (index >= 0) {
                  SchedulerBinding.instance!.addPostFrameCallback((_) => _animateToIndex(index));
                }
              }
            },
            disconnected: (_) => _refreshController.refreshCompleted(),
            close: (_) => Navigator.of(ctx).pop(),
            orElse: () {},
          ),
          builder: (ctx, state) => state.map(
            loading: (_) => const PlayListContentLoading(),
            loaded: (state) => PlayListContentLoaded(
              playListId: state.playlistId,
              isLoaded: state.loaded,
              files: state.isFiltering ? state.filteredFiles : state.files,
              searchBoxIsVisible: state.searchBoxIsVisible,
              itemHeight: _itemHeight,
              refreshController: _refreshController,
              listViewScrollController: _listViewScrollController,
            ),
            disconnected: (_) => const PlayListContentDisconnected(),
            close: (_) => Container(),
            notFound: (_) => const PlayListContentNotFound(),
          ),
        ),
      ),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerFloat,
      floatingActionButton: BlocBuilder<PlayListBloc, PlayListState>(
        builder: (ctx, state) => state.maybeMap(
          loaded: (state) => PlayListFab(
            id: state.playlistId,
            name: state.name,
            loop: state.loop,
            shuffle: state.shuffle,
            hideFabAnimController: _hideFabAnimController,
            //TODO: THIS WIDGET IS BEING REBUILD BECAUSE OF THE FUNCTION
            onArrowTopTap: () => _animateToIndex(1),
          ),
          orElse: () => nil,
        ),
      ),
    );
  }

  @override
  void dispose() {
    _hideFabAnimController.dispose();
    _listViewScrollController.dispose();
    _refreshController.dispose();
    super.dispose();
  }

  Future<void> _animateToIndex(int i) async {
    final offset = _itemHeight * i;
    await _listViewScrollController.animateTo(offset, duration: const Duration(seconds: 2), curve: Curves.fastOutSlowIn);
  }

  void _onListViewScroll() {
    switch (_listViewScrollController.position.userScrollDirection) {
      case ScrollDirection.idle:
        break;
      case ScrollDirection.forward:
        _hideFabAnimController.forward();
        break;
      case ScrollDirection.reverse:
        _hideFabAnimController.reverse();
        break;
    }
  }
}
