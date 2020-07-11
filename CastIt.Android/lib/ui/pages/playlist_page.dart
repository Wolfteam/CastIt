import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../common/styles.dart';
import '../../generated/i18n.dart';
import '../../models/dtos/responses/file_item_response_dto.dart';
import '../widgets/items/file_item.dart';
import '../widgets/items/item_counter.dart';
import '../widgets/something_went_wrong.dart';

class PlayListPage extends StatefulWidget {
  final int id;
  final int scrollToFileId;
  const PlayListPage({
    @required this.id,
    this.scrollToFileId,
  });

  @override
  _PlayListPageState createState() => _PlayListPageState();
}

class _PlayListPageState extends State<PlayListPage> with SingleTickerProviderStateMixin {
  final _refreshController = RefreshController(initialRefresh: false);

  final _listViewScrollController = ScrollController();

  final _itemHeight = 75.0;

  final _searchFocusNode = FocusNode();

  TextEditingController _searchBoxTextController;

  AnimationController _hideFabAnimController;

  @override
  void initState() {
    super.initState();

    _searchBoxTextController = TextEditingController(text: '');
    _hideFabAnimController = AnimationController(
      vsync: this,
      duration: kThemeAnimationDuration,
      value: 1, // initially visible
    );
    _searchBoxTextController.addListener(_onSearchTextChanged);
    _listViewScrollController.addListener(_onListViewScroll);
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<PlayListBloc, PlayListState>(
      listener: (ctx, state) {
        state.maybeMap(
          loaded: (_) => _refreshController.refreshCompleted(),
          disconnected: (_) => _refreshController.refreshCompleted(),
          orElse: () {},
        );
      },
      builder: (_, state) => Scaffold(
        body: SafeArea(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              ..._buildPage(state),
            ],
          ),
        ),
        floatingActionButtonLocation: FloatingActionButtonLocation.centerFloat,
        floatingActionButton: _buildFloatingActionBar(state),
      ),
    );
  }

  @override
  void dispose() {
    _hideFabAnimController.dispose();
    _searchBoxTextController.dispose();
    _listViewScrollController.dispose();
    _refreshController.dispose();
    super.dispose();
  }

  List<Widget> _buildPage(PlayListState state) {
    final i18n = I18n.of(context);
    return state.map<List<Widget>>(
      disconnected: (_) {
        return [
          _buildHeader(),
          const SomethingWentWrong(),
        ];
      },
      loading: (_) {
        return [
          _buildHeader(),
          const Expanded(
            child: Center(
              child: CircularProgressIndicator(),
            ),
          ),
        ];
      },
      loaded: (s) {
        if (!s.loaded) {
          return [
            _buildHeader(),
            Expanded(
              child: Center(
                child: Text(i18n.somethingWentWrong),
              ),
            ),
          ];
        }
        if (widget.scrollToFileId != null) {
          final id = s.files.firstWhere((element) => element.id == widget.scrollToFileId, orElse: () => null)?.id;
          if (id != null) {
            SchedulerBinding.instance.addPostFrameCallback((_) => _animateToIndex(id));
          }
        }
        final filesToUse = s.isFiltering ? s.filteredFiles : s.files;
        return [
          _buildHeader(itemCount: filesToUse.length, showSearch: s.searchBoxIsVisible),
          _buildItems(filesToUse),
        ];
      },
    );
  }

  Widget _buildHeader({int itemCount, bool showSearch = false}) {
    final i18n = I18n.of(context);
    if (showSearch) {
      WidgetsBinding.instance.addPostFrameCallback((timeStamp) {
        _searchFocusNode.requestFocus();
      });
    }
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 10),
      child: Column(
        children: <Widget>[
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: <Widget>[
              Align(
                alignment: Alignment.centerLeft,
                child: FlatButton.icon(
                  icon: Icon(Icons.arrow_back),
                  onPressed: () {
                    Navigator.of(context).pop();
                  },
                  label: Text(
                    i18n.playlists,
                    style: const TextStyle(fontSize: 24),
                  ),
                ),
              ),
              if (itemCount != null)
                Container(
                  margin: const EdgeInsets.only(right: 20),
                  child: ItemCounter(itemCount),
                ),
            ],
          ),
          if (showSearch) _buildSearchBox(),
        ],
      ),
    );
  }

  Widget _buildItems(List<FileItemResponseDto> files) {
    final listView = SmartRefresher(
      enablePullDown: true,
      header: const MaterialClassicHeader(),
      controller: _refreshController,
      onRefresh: () {
        context.bloc<PlayListBloc>().add(PlayListEvent.load(id: widget.id));
      },
      child: ListView.builder(
        controller: _listViewScrollController,
        shrinkWrap: true,
        itemCount: files.length,
        itemBuilder: (ctx, i) {
          final file = files[i];
          return FileItem(
            key: _getKeyForFileItem(file.id),
            itemHeight: _itemHeight,
            id: file.id,
            position: file.position,
            playListId: file.playListId,
            isBeingPlayed: file.isBeingPlayed,
            totalSeconds: file.totalSeconds,
            name: file.filename,
            path: file.path,
            size: file.size,
            ext: file.ext,
            exists: file.exists,
            isLocalFile: file.isLocalFile,
            isUrlFile: file.isUrlFile,
            playedPercentage: file.playedPercentage,
            loop: file.loop,
          );
        },
      ),
    );

    return Expanded(child: listView);
  }

  Widget _buildSearchBox() {
    final theme = Theme.of(context);
    final i18n = I18n.of(context);
    return Card(
      elevation: 10,
      margin: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
      shape: Styles.floatingCardShape,
      child: Row(
        children: <Widget>[
          Container(
            margin: const EdgeInsets.only(left: 10),
            child: Icon(Icons.search, size: 30),
          ),
          Expanded(
            child: TextField(
              controller: _searchBoxTextController,
              focusNode: _searchFocusNode,
              cursorColor: theme.accentColor,
              keyboardType: TextInputType.text,
              textInputAction: TextInputAction.go,
              decoration: InputDecoration(
                border: InputBorder.none,
                contentPadding: const EdgeInsets.symmetric(horizontal: 15),
                hintText: '${i18n.search}...',
              ),
            ),
          ),
          IconButton(
            icon: Icon(Icons.close),
            onPressed: _cleanSearchText,
          ),
        ],
      ),
    );
  }

  Widget _buildFloatingActionBar(PlayListState state) {
    final theme = Theme.of(context);
    const iconSize = 30.0;
    return state.map(
      disconnected: (s) => null,
      loading: (s) => null,
      loaded: (s) => FadeTransition(
        opacity: _hideFabAnimController,
        child: Card(
          elevation: 10,
          shape: Styles.floatingCardShape,
          margin: const EdgeInsets.symmetric(horizontal: 20),
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 10),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: <Widget>[
                ButtonBar(
                  buttonPadding: const EdgeInsets.all(0),
                  children: <Widget>[
                    IconButton(
                      icon: Icon(Icons.loop, color: s.loop ? theme.accentColor : null, size: iconSize),
                      onPressed: () => _setPlayListOptions(!s.loop, s.shuffle),
                    ),
                    IconButton(
                      icon: Icon(Icons.shuffle, color: s.shuffle ? theme.accentColor : null, size: iconSize),
                      onPressed: () => _setPlayListOptions(s.loop, !s.shuffle),
                    ),
                  ],
                ),
                Expanded(
                  child: Text(s.name, textAlign: TextAlign.center, overflow: TextOverflow.ellipsis),
                ),
                ButtonBar(
                  buttonPadding: const EdgeInsets.all(0),
                  children: <Widget>[
                    IconButton(
                      icon: Icon(Icons.search, size: iconSize),
                      onPressed: _toggleSearchBoxVisibility,
                    ),
                    IconButton(
                      icon: Icon(Icons.arrow_upward, size: iconSize),
                      onPressed: () => _animateToIndex(1),
                    ),
                  ],
                )
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _setPlayListOptions(bool loop, bool shuffle) {
    final bloc = context.bloc<ServerWsBloc>();
    bloc.setPlayListOptions(widget.id, loop: loop, shuffle: shuffle);
    context.bloc<PlayListBloc>().add(PlayListEvent.playListOptionsChanged(loop: loop, shuffle: shuffle));
  }

  void _animateToIndex(int i) => _listViewScrollController.animateTo(
        (_itemHeight * i) - _itemHeight,
        duration: const Duration(seconds: 2),
        curve: Curves.fastOutSlowIn,
      );

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

  void _toggleSearchBoxVisibility() =>
      context.bloc<PlayListBloc>().add(const PlayListEvent.toggleSearchBoxVisibility());

  void _onSearchTextChanged() =>
      context.bloc<PlayListBloc>().add(PlayListEvent.searchBoxTextChanged(text: _searchBoxTextController.text));

  void _cleanSearchText() {
    if (_searchBoxTextController.text.isEmpty) {
      _toggleSearchBoxVisibility();
      return;
    }
    _searchBoxTextController.text = '';
  }

  Key _getKeyForFileItem(int id) => Key('file_item_$id');
}
