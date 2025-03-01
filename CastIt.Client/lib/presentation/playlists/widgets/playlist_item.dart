import 'package:castit/application/bloc.dart';
import 'package:castit/presentation/playlist/playlist_page.dart';
import 'package:castit/presentation/playlist/widgets/item_counter.dart';
import 'package:castit/presentation/playlists/widgets/playlist_options_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
    super.key,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 10, vertical: 10),
      child: ListTile(
        leading: const Icon(Icons.list, size: 36),
        title: Text(
          name,
          style: theme.textTheme.titleLarge,
          overflow: TextOverflow.ellipsis,
        ),
        subtitle: Row(
          children: [
            const Icon(Icons.hourglass_empty, size: 18),
            BlocBuilder<PlayedPlayListItemBloc, PlayedPlayListItemState>(
              builder: (ctx, state) => Text(
                state.maybeMap(
                  playing: (state) => state.id == id ? state.totalDuration : totalDuration,
                  orElse: () => totalDuration,
                ),
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ],
        ),
        trailing: ItemCounter(numberOfFiles),
        onLongPress: () => _showPlayListOptionsModal(context),
        onTap: () => _goToPlayListPage(context),
      ),
    );
  }

  void _showPlayListOptionsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => PlayListOptionsBottomSheetDialog(playListId: id, playListName: name),
    );
  }

  Future<void> _goToPlayListPage(BuildContext context) async {
    await PlayListPage.forDetails(id, null, context);
  }
}
