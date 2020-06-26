import 'package:castit/models/dtos/responses/playlist_response_dto.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/playlist/playlist_bloc.dart';
import '../pages/playlist_page.dart';
import 'item_counter.dart';

class PlayListItem extends StatelessWidget {
  final PlayListResponseDto playlist;

  const PlayListItem({
    @required this.playlist,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(Icons.list),
      title: Text(playlist.name),
      trailing: ItemCounter(playlist.numberOfFiles),
      onTap: () {
        context.bloc<PlaylistBloc>().add(LoadPlayList(playlist: playlist));
        final route = MaterialPageRoute(
          builder: (ctx) => PlayListPage(playlist),
        );
        Navigator.push(context, route);
      },
    );
  }
}
