import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../../models/dtos/responses/file_response_dto.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';
import '../widgets/file_item.dart';
import '../widgets/item_counter.dart';

class PlayListPage extends StatelessWidget {
  final _refreshController = RefreshController(initialRefresh: false);
  final PlayListResponseDto _playlist;
  PlayListPage(
    this._playlist,
  );

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
              Container(
                margin: EdgeInsets.only(top: 10, left: 10),
                child: Align(
                  alignment: Alignment.centerLeft,
                  child: OutlineButton.icon(
                    icon: Icon(Icons.arrow_back),
                    onPressed: () {
                      Navigator.of(context).pop();
                    },
                    label: Text("PlayLists"),
                  ),
                ),
              ),
              ..._buildPage(ctx, state),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildPage(BuildContext context, PlayListState state) {
    return state.when<List<Widget>>(
      loading: () {
        return [
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
            Expanded(
              child: Center(
                child: Text('Something went wrong'),
              ),
            ),
          ];
        }

        return [
          _buildHeader(context, name, files.length),
          _buildActionButtons(),
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
    final theme = Theme.of(context);
    return Container(
      margin: EdgeInsets.only(top: 5, right: 10, left: 10),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: <Widget>[
          Row(
            children: <Widget>[
              Icon(
                Icons.list,
                size: 40,
              ),
              Text(
                playListName,
                style: theme.textTheme.headline4,
              ),
            ],
          ),
          ItemCounter(itemLength),
        ],
      ),
    );
  }

  Widget _buildActionButtons() {
    return Row(
      mainAxisAlignment: MainAxisAlignment.start,
      children: <Widget>[
        IconButton(
          icon: Icon(Icons.shuffle),
          onPressed: () {},
        ),
        IconButton(
          icon: Icon(Icons.loop),
          onPressed: () {},
        ),
        IconButton(
          icon: Icon(Icons.search),
          onPressed: () {},
        )
      ],
    );
  }

  Widget _buildItems(BuildContext context, List<FileResponseDto> files) {
    return Expanded(
      child: SmartRefresher(
        enablePullDown: true,
        header: const MaterialClassicHeader(),
        controller: _refreshController,
        onRefresh: () {
          context
              .bloc<PlayListBloc>()
              .add(PlayListEvent.load(playList: _playlist));
        },
        child: ListView.builder(
          shrinkWrap: true,
          itemCount: files.length,
          itemBuilder: (ctx, i) {
            final file = files[i];
            return FileItem(
              file.position,
              file.filename,
              file.path,
              file.size,
              file.ext,
            );
          },
        ),
      ),
    );
  }
}
