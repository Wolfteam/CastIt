import 'package:flutter/material.dart';

import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'delete_playlist_bottom_sheet.dart';
import 'modal_sheet_separator.dart';
import 'rename_playlist_bottom_sheet.dart';

class PlayListOptionsBottomSheetDialog extends StatelessWidget {
  final int playListId;
  final String playListName;

  const PlayListOptionsBottomSheetDialog({
    @required this.playListId,
    @required this.playListName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = I18n.of(context);
    return SingleChildScrollView(
      padding: EdgeInsets.only(bottom: MediaQuery.of(context).viewInsets.bottom),
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: <Widget>[
            ModalSheetSeparator(),
            BottomSheetTitle(icon: Icons.playlist_play, title: i18n.playlistOptions),
            TextButton(
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
      ),
    );
  }

  void _showRenameModal(BuildContext context) {
    Navigator.of(context).pop();
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => RenamePlayListBottomSheet(id: playListId, currentName: playListName),
    );
  }

  void _showDeleteModal(BuildContext context) {
    Navigator.of(context).pop();
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => DeletePlayListBottomSheet(playListId: playListId, playListName: playListName),
    );
  }
}
