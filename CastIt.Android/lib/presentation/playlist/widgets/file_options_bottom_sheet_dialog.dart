import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/bottom_sheet_title.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:castit/presentation/shared/modal_sheet_separator.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'delete_file_bottom_sheet.dart';

class FileOptionsBottomSheetDialog extends StatelessWidget {
  final int id;
  final int playListId;
  final String fileName;

  const FileOptionsBottomSheetDialog({
    required this.id,
    required this.playListId,
    required this.fileName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context)!;
    final theme = Theme.of(context);
    final textColor = theme.brightness == Brightness.dark ? Colors.white : Colors.black;
    return SingleChildScrollView(
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: <Widget>[
            ModalSheetSeparator(),
            BottomSheetTitle(icon: Icons.insert_drive_file, title: i18n.fileOptions),
            TextButton(
              style: TextButton.styleFrom(primary: textColor),
              onPressed: () => _playFile(context, false),
              child: Row(
                children: <Widget>[
                  const Icon(Icons.play_arrow),
                  const SizedBox(width: 10),
                  Text(i18n.playFile),
                ],
              ),
            ),
            TextButton(
              style: TextButton.styleFrom(primary: textColor),
              onPressed: () => _playFile(context, true),
              child: Row(
                children: <Widget>[
                  const Icon(Icons.play_circle_filled),
                  const SizedBox(width: 10),
                  Text(i18n.playFileFromTheBeginning),
                ],
              ),
            ),
            TextButton(
              style: TextButton.styleFrom(primary: textColor),
              onPressed: () => _showDeleteModal(context),
              child: Row(
                children: <Widget>[
                  const Icon(Icons.delete),
                  const SizedBox(width: 10),
                  Text(i18n.deleteFile),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _playFile(BuildContext context, bool force) {
    Navigator.of(context).pop(true);
    return context.read<ServerWsBloc>().playFile(id, playListId, force: force);
  }

  void _showDeleteModal(BuildContext context) {
    Navigator.of(context).pop(false);
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => DeleteFileBottomSheet(id: id, playListId: playListId, fileName: fileName),
    );
  }
}
