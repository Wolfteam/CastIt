import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/server_ws/server_ws_bloc.dart';
import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'delete_file_bottom_sheet.dart';
import 'modal_sheet_separator.dart';

class FileOptionsBottomSheetDialog extends StatelessWidget {
  final int id;
  final int playListId;
  final String fileName;

  const FileOptionsBottomSheetDialog({
    @required this.id,
    @required this.playListId,
    @required this.fileName,
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
            BottomSheetTitle(icon: Icons.insert_drive_file, title: i18n.fileOptions),
            FlatButton(
              onPressed: () => _playFile(context, false),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.play_arrow),
                  const SizedBox(width: 10),
                  Text(i18n.playFile),
                ],
              ),
            ),
            FlatButton(
              onPressed: () => _playFile(context, true),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.play_circle_filled),
                  const SizedBox(width: 10),
                  Text(i18n.playFileFromTheBeginning),
                ],
              ),
            ),
            FlatButton(
              onPressed: () => _showDeleteModal(context),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.delete),
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
    return context.bloc<ServerWsBloc>().playFile(id, playListId, force: force);
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
