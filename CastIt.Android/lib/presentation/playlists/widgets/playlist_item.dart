import 'package:flutter/material.dart';

import 'playlist_options_bottom_sheet_dialog.dart';
import '../../playlist/playlist_page.dart';
import '../../playlist/widgets/item_counter.dart';
import '../../shared/extensions/styles.dart';

class PlayListItem extends StatelessWidget {
  final int id;
  final String name;
  final int numberOfFiles;
  final bool loop;
  final bool shuffle;
  final String totalDuration;

  const PlayListItem({
    required this.id,
    required this.name,
    required this.numberOfFiles,
    required this.loop,
    required this.shuffle,
    required this.totalDuration,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return ListTile(
      leading: const Icon(Icons.list, size: 36),
      title: Text(
        name,
        style: theme.textTheme.headline6,
        overflow: TextOverflow.ellipsis,
      ),
      subtitle: Row(
        children: [
          const Icon(Icons.hourglass_empty, size: 18),
          Text(totalDuration, overflow: TextOverflow.ellipsis),
        ],
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

  Future<void> _goToPlayListPage(BuildContext context) async {
    await PlayListPage.forDetails(id, null, context);
  }
}
