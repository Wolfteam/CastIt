import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlists/widgets/playlist_item.dart';
import 'package:castit/presentation/shared/page_header.dart';
import 'package:castit/presentation/shared/something_went_wrong.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

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
            listener: (ctx, state) {
              switch (state) {
                case PlayListsStateLoadedState():
                  _refreshController.refreshCompleted();
                case PlayListsStateDisconnectedState():
                  _refreshController.refreshCompleted();
                  context.read<ServerWsBloc>().add(const ServerWsEvent.disconnectedFromWs());
                default:
                  break;
              }
            },
            builder: (ctx, state) => SmartRefresher(
              header: const MaterialClassicHeader(),
              controller: _refreshController,
              onRefresh: () => context.read<PlayListsBloc>().add(const PlayListsEvent.load()),
              child: switch (state) {
                PlayListsStateLoadingState() => const Center(child: CircularProgressIndicator()),
                //TODO: CREATE A WAY TO FORCE A RECONNECT
                // context.read<ServerWsBloc>().add(ServerWsEvent.connectToWs());
                PlayListsStateLoadedState() => ListView.builder(
                  itemCount: state.playlists.length,
                  itemBuilder: (ctx, i) {
                    final playlist = state.playlists[i];
                    return PlayListItem(
                      key: Key('playlist_$i'),
                      id: playlist.id,
                      name: playlist.name,
                      numberOfFiles: playlist.numberOfFiles,
                      loop: playlist.loop,
                      shuffle: playlist.shuffle,
                      totalDuration: playlist.totalDuration,
                      lastPlayedDate: playlist.lastPlayedDate,
                    );
                  },
                ),
                PlayListsStateDisconnectedState() => const SomethingWentWrong(),
              },
            ),
          ),
        ),
      ],
    );
  }
}
