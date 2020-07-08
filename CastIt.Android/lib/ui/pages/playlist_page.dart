import 'package:flutter/material.dart';
import 'package:flutter/scheduler.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../generated/i18n.dart';
import '../../models/dtos/responses/file_item_response_dto.dart';
import '../widgets/items/file_item.dart';
import '../widgets/items/item_counter.dart';
import '../widgets/page_header.dart';

class PlayListPage extends StatelessWidget {
  final _refreshController = RefreshController(initialRefresh: false);
  final _listViewScrollController = ScrollController();
  final _itemHeight = 75.0;

  final int id;
  final int scrollToFileId;
  PlayListPage({
    @required this.id,
    this.scrollToFileId,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: BlocConsumer<PlayListBloc, PlayListState>(
          listener: (ctx, state) {
            state.maybeMap(
              loaded: (_) {
                _refreshController.refreshCompleted();
              },
              orElse: () {},
            );
          },
          builder: (ctx, state) => Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              ..._buildPage(ctx, state),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildPage(BuildContext context, PlayListState state) {
    final i18n = I18n.of(context);
    final goBack = Container(
      margin: const EdgeInsets.symmetric(vertical: 10),
      child: Align(
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
    );
    return state.when<List<Widget>>(
      loading: () {
        return [
          goBack,
          const Expanded(
            child: Center(
              child: CircularProgressIndicator(),
            ),
          ),
        ];
      },
      loaded: (playListId, name, position, loop, shuffle, files, loaded) {
        if (!loaded) {
          return [
            goBack,
            Expanded(
              child: Center(
                child: Text(i18n.somethingWentWrong),
              ),
            ),
          ];
        }
        if (scrollToFileId != null) {
          final id = files.firstWhere((element) => element.id == scrollToFileId, orElse: () => null)?.id;
          if (id != null) {
            SchedulerBinding.instance.addPostFrameCallback((_) => _animateToIndex(id));
          }
        }
        return [
          goBack,
          _buildHeader(context, name, files.length),
          _buildActionButtons(context, shuffle, loop),
          _buildItems(context, files),
        ];
      },
    );
  }

  Widget _buildHeader(
    BuildContext context,
    String playListName,
    int itemLength,
  ) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: <Widget>[
        Flexible(
          child: PageHeader(
            margin: const EdgeInsets.symmetric(horizontal: 10),
            title: playListName,
            icon: Icons.list,
          ),
        ),
        Container(
          alignment: Alignment.centerRight,
          margin: const EdgeInsets.only(right: 10),
          child: ItemCounter(itemLength),
        ),
      ],
    );
  }

  Widget _buildActionButtons(BuildContext ctx, bool shuffle, bool loop) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: <Widget>[
        ToggleButtons(
          borderColor: Colors.transparent,
          fillColor: Colors.transparent,
          selectedBorderColor: Colors.transparent,
          onPressed: (int index) {
            final s = index == 0 ? !shuffle : shuffle;
            final l = index == 1 ? !loop : loop;
            _setPlayListOptions(ctx, l, s);
          },
          isSelected: [shuffle, loop],
          children: <Widget>[
            Icon(Icons.shuffle),
            Icon(Icons.loop),
          ],
        ),
        // IconButton(
        //   icon: Icon(Icons.shuffle),
        //   onPressed: () => _setPlayListOptions(ctx, shuffle, loop),
        // ),
        // IconButton(
        //   icon: Icon(Icons.loop,),
        //   onPressed: () => _setPlayListOptions(ctx, shuffle, loop),
        // ),
        IconButton(
          icon: Icon(Icons.search),
          onPressed: () {},
        )
      ],
    );
  }

  Widget _buildItems(BuildContext context, List<FileItemResponseDto> files) {
    return Expanded(
      child: SmartRefresher(
        enablePullDown: true,
        header: const MaterialClassicHeader(),
        controller: _refreshController,
        onRefresh: () {
          context.bloc<PlayListBloc>().add(PlayListEvent.load(id: id));
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
      ),
    );
  }

  void _setPlayListOptions(BuildContext ctx, bool loop, bool shuffle) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.setPlayListOptions(id, loop: loop, shuffle: shuffle);
  }

//TODO: UPDATE THE PROGRESS
//TODO: IF THE PLAYED FILE CHANGES, AND THIS PAGE IS OPEN, SCROLL TO THE NEW PLAYED FILE
  void _animateToIndex(int i) => _listViewScrollController.animateTo(
        (_itemHeight * i) - _itemHeight,
        duration: const Duration(seconds: 2),
        curve: Curves.fastOutSlowIn,
      );

  Key _getKeyForFileItem(int id) => Key('file_item_$id');
}
