import 'package:flutter/material.dart';

import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'modal_sheet_separator.dart';

class PlayListOptionsBottomSheetDialog extends StatelessWidget {
  const PlayListOptionsBottomSheetDialog();
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
            BottomSheetTitle(icon: Icons.playlist_play, title: 'PlayList Options'),
            FlatButton(
              onPressed: () {},
              child: Row(
                mainAxisAlignment: MainAxisAlignment.start,
                children: <Widget>[
                  Icon(Icons.edit),
                  const SizedBox(width: 10),
                  Text(i18n.rename),
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
                  Text(i18n.delete),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
