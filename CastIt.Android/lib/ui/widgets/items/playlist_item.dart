import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/playlist/playlist_bloc.dart';
import '../../../common/styles.dart';
import '../../pages/playlist_page.dart';
import '../modals/playlist_options_bottom_sheet_dialog.dart';
import 'item_counter.dart';

class PlayListItem extends StatelessWidget {
  final int id;
  final String name;
  final int numberOfFiles;
  final bool loop;
  final bool shuffle;

  const PlayListItem({
    @required this.id,
    @required this.name,
    @required this.numberOfFiles,
    @required this.loop,
    @required this.shuffle,
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
        name,
        style: theme.textTheme.headline6,
        overflow: TextOverflow.ellipsis,
      ),
      trailing: ItemCounter(numberOfFiles),
      onLongPress: () => _showPlayListOptionsModal(context),
      onTap: () => _goToPlayListPage(context),
    );
  }

  void _showPlayListOptionsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => PlayListOptionsBottomSheetDialog(playListId: id, playListName: name),
    );
  }

  void _goToPlayListPage(BuildContext context) {
    context.bloc<PlayListBloc>().add(PlayListEvent.load(id: id));
    final route = MaterialPageRoute(
      builder: (ctx) => PlayListPage(id: id),
    );
    Navigator.push(context, route);
  }
}
