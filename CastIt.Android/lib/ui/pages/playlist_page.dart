import 'package:castit/bloc/server_ws/server_ws_bloc.dart';
import 'package:castit/generated/i18n.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../../models/dtos/responses/file_response_dto.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';
import '../widgets/file_item.dart';
import '../widgets/item_counter.dart';
import '../widgets/page_header.dart';

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

        return [
          goBack,
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
          context.bloc<PlayListBloc>().add(PlayListEvent.load(playList: _playlist));
        },
        child: ListView.builder(
          shrinkWrap: true,
          itemCount: files.length,
          itemBuilder: (ctx, i) {
            final file = files[i];
            return FileItem(
              file.position,
              file.id,
              file.playListId,
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

  void _setPlayListOptions(BuildContext ctx, bool loop, bool shuffle) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.setPlayListOptions(_playlist.id, loop: loop, shuffle: shuffle);
  }
}
