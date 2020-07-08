import 'package:flutter/material.dart';

import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'modal_sheet_separator.dart';

class FileOptionsBottomSheetDialog extends StatelessWidget {
  const FileOptionsBottomSheetDialog();
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
            BottomSheetTitle(icon: Icons.insert_drive_file, title: 'File Options'),
            FlatButton(
              onPressed: () {},
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.play_arrow),
                  const SizedBox(width: 10),
                  Text(
                    i18n.playFile,
                  ),
                ],
              ),
            ),
            FlatButton(
              onPressed: () {},
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.play_circle_filled),
                  const SizedBox(width: 10),
                  Text(
                    i18n.playFileFromTheBeginning,
                  ),
                ],
              ),
            ),
            FlatButton(
              onPressed: () {},
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
}
