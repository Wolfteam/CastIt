import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlists/playlists_bloc.dart';
import '../widgets/playlist_item.dart';

class PlayListsPage extends StatelessWidget {
  final _refreshController = RefreshController(initialRefresh: false);

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Container(
          margin: EdgeInsets.symmetric(vertical: 20, horizontal: 30),
          child: Text(
            'PlayLists',
            textAlign: TextAlign.start,
            style: TextStyle(
              fontSize: 28,
            ),
          ),
        ),
        Expanded(
          child: BlocConsumer<PlaylistsBloc, PlaylistsState>(
            listener: (ctx, state) {
              if (state is PlayListsLoadedState) {
                _refreshController.refreshCompleted();
              }
            },
            builder: (ctx, state) => SmartRefresher(
              enablePullDown: true,
              header: const MaterialClassicHeader(),
              controller: _refreshController,
              onRefresh: () {
                context.bloc<PlaylistsBloc>().add(LoadPlayLists());
              },
              child: _buildPage(ctx, state),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildPage(BuildContext context, PlaylistsState state) {
    if (state is PlayListsLoadedState) {
      if (!state.loaded) {
        return Container(
          child: Text(
            'Something went wrong!',
          ),
        );
      }

      return ListView.builder(
        itemCount: state.playlists.length,
        itemBuilder: (ctx, i) {
          final playlist = state.playlists[i];
          return Card(child: PlayListItem(playlist: playlist));
        },
      );
    }
    return const Center(
      child: CircularProgressIndicator(),
    );
  }
}
