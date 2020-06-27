import 'package:castit/generated/i18n.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import '../../bloc/playlists/playlists_bloc.dart';
import '../widgets/playlist_item.dart';
import '../widgets/page_header.dart';

class PlayListsPage extends StatefulWidget {
  @override
  _PlayListsPageState createState() => _PlayListsPageState();
}

class _PlayListsPageState extends State<PlayListsPage>
    with AutomaticKeepAliveClientMixin<PlayListsPage> {
  final _refreshController = RefreshController(initialRefresh: false);

  @override
  bool get wantKeepAlive => true;

  @override
  Widget build(BuildContext context) {
    super.build(context);
    final i18n = I18n.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        PageHeader(
          title: i18n.playlists,
          icon: Icons.library_music,
        ),
        Expanded(
          child: BlocConsumer<PlayListsBloc, PlayListsState>(
            listener: (ctx, state) {
              state.maybeMap(
                loaded: (_) {
                  _refreshController.refreshCompleted();
                },
                orElse: () {},
              );
            },
            builder: (ctx, state) => SmartRefresher(
              enablePullDown: true,
              header: const MaterialClassicHeader(),
              controller: _refreshController,
              onRefresh: () {
                context.bloc<PlayListsBloc>().add(PlayListsEvent.load());
              },
              child: _buildPage(ctx, state),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildPage(BuildContext context, PlayListsState state) {
    return state.when(
      loading: () {
        return const Center(
          child: CircularProgressIndicator(),
        );
      },
      loaded: (loaded, playlists, _) {
        if (!loaded) {
          return Container(
            child: Text(
              'Something went wrong!',
            ),
          );
        }

        return ListView.builder(
          itemCount: playlists.length,
          itemBuilder: (ctx, i) {
            final playlist = playlists[i];
            return Card(child: PlayListItem(playlist: playlist));
          },
        );
      },
    );
  }
}
