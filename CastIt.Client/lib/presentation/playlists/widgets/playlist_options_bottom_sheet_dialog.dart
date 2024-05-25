import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlists/widgets/delete_playlist_bottom_sheet.dart';
import 'package:castit/presentation/playlists/widgets/rename_playlist_bottom_sheet.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';

class PlayListOptionsBottomSheetDialog extends StatelessWidget {
  final int playListId;
  final String playListName;

  const PlayListOptionsBottomSheetDialog({
    required this.playListId,
    required this.playListName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    final textColor = theme.brightness == Brightness.dark ? Colors.white : Colors.black;
    return CommonBottomSheet(
      title: i18n.playlistOptions,
      titleIcon: Icons.playlist_play,
      showOkButton: false,
      showCancelButton: false,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: <Widget>[
          TextButton(
            style: TextButton.styleFrom(foregroundColor: textColor),
            onPressed: () => _showRenameModal(context),
            child: Row(
              children: <Widget>[
                const Icon(Icons.edit),
                const SizedBox(width: 10),
                Text(i18n.rename),
              ],
            ),
          ),
          TextButton(
            style: TextButton.styleFrom(foregroundColor: textColor),
            onPressed: () => _showDeleteModal(context),
            child: Row(
              children: <Widget>[
                const Icon(Icons.delete),
                const SizedBox(width: 10),
                Text(i18n.delete),
              ],
            ),
          ),
        ],
      ),
    );
  }

  void _showRenameModal(BuildContext context) {
    Navigator.of(context).pop();
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => RenamePlayListBottomSheet(id: playListId, currentName: playListName),
    );
  }

  void _showDeleteModal(BuildContext context) {
    Navigator.of(context).pop();
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => DeletePlayListBottomSheet(playListId: playListId, playListName: playListName),
    );
  }
}
