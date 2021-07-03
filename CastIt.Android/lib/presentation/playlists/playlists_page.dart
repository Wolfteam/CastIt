import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/page_header.dart';
import 'package:castit/presentation/shared/something_went_wrong.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

import 'widgets/playlist_item.dart';

class PlayListsPage extends StatefulWidget {
  @override
  _PlayListsPageState createState() => _PlayListsPageState();
}

class _PlayListsPageState extends State<PlayListsPage> with AutomaticKeepAliveClientMixin<PlayListsPage> {
  final _refreshController = RefreshController();

  @override
  bool get wantKeepAlive => true;

  @override
  Widget build(BuildContext context) {
    super.build(context);
    final i18n = S.of(context);
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        PageHeader(title: i18n.playlists, icon: Icons.library_music),
        Expanded(
          child: BlocConsumer<PlayListsBloc, PlayListsState>(
            listener: (ctx, state) => state.maybeMap(
              loaded: (_) => _refreshController.refreshCompleted(),
              disconnected: (_) => _refreshController.refreshCompleted(),
              orElse: () {},
            ),
            builder: (ctx, state) => SmartRefresher(
              header: const MaterialClassicHeader(),
              controller: _refreshController,
              onRefresh: () => context.read<PlayListsBloc>().add(PlayListsEvent.load()),
              child: state.when(
                loading: () => const Center(child: CircularProgressIndicator()),
                loaded: (playlists, _) {
                  //TODO: CREATE A WAY TO FORCE A RECONNECT
                  // context.read<ServerWsBloc>().add(ServerWsEvent.connectToWs());
                  return ListView.builder(
                    itemCount: playlists.length,
                    itemBuilder: (ctx, i) {
                      final playlist = playlists[i];
                      return PlayListItem(
                        key: Key('playlist_$i'),
                        id: playlist.id,
                        name: playlist.name,
                        numberOfFiles: playlist.numberOfFiles,
                        loop: playlist.loop,
                        shuffle: playlist.shuffle,
                        totalDuration: playlist.totalDuration,
                      );
                    },
                  );
                },
                disconnected: () {
                  context.read<ServerWsBloc>().add(ServerWsEvent.disconnectedFromWs());
                  return const SomethingWentWrong();
                },
              ),
            ),
          ),
        ),
      ],
    );
  }
}
