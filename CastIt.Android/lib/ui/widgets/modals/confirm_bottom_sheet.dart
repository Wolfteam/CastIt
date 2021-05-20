import 'package:flutter/material.dart';

import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'modal_sheet_separator.dart';

class ConfirmBottomSheet extends StatelessWidget {
  final String title;
  final IconData icon;
  final String content;
  final String? okText;
  final String? cancelText;
  final Function onOk;
  final Function onCancel;

  const ConfirmBottomSheet({
    Key? key,
    required this.title,
    required this.icon,
    required this.content,
    required this.onOk,
    required this.onCancel,
    this.okText,
    this.cancelText,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final i18n = I18n.of(context);
    final theme = Theme.of(context);
    final separator = ModalSheetSeparator();
    final sheetTitle = BottomSheetTitle(icon: icon, title: title);
    return SingleChildScrollView(
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: <Widget>[
            separator,
            sheetTitle,
            Padding(
              padding: const EdgeInsets.only(top: 10, left: 10),
              child: Text(content, style: theme.textTheme.subtitle1),
            ),
            ButtonBar(
              buttonPadding: const EdgeInsets.symmetric(horizontal: 10),
              children: <Widget>[
                OutlinedButton(
                  onPressed: () => onCancel(),
                  child: Text(cancelText ?? i18n!.cancel, style: TextStyle(color: theme.primaryColor)),
                ),
                ElevatedButton(
                  onPressed: () => onOk(),
                  child: Text(okText ?? i18n!.ok),
                )
              ],
            )
          ],
        ),
      ),
    );
  }
}
