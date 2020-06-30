import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';
import '../pages/playlist_page.dart';
import 'item_counter.dart';

class PlayListItem extends StatelessWidget {
  final PlayListResponseDto playlist;

  const PlayListItem({
    @required this.playlist,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListTile(
      leading: Icon(
        Icons.list,
        size: 36,
      ),
      title: Text(
        playlist.name,
        style: theme.textTheme.headline6,
        overflow: TextOverflow.ellipsis,
      ),
      trailing: ItemCounter(playlist.numberOfFiles),
      onTap: () {
        context
            .bloc<PlayListBloc>()
            .add(PlayListEvent.load(playList: playlist));
        final route = MaterialPageRoute(
          builder: (ctx) => PlayListPage(playlist),
        );
        Navigator.push(context, route);
      },
    );
  }
}
