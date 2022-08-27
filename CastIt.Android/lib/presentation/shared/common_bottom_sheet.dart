import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/bottom_sheet_title.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:castit/presentation/shared/modal_sheet_separator.dart';
import 'package:flutter/material.dart';

class CommonBottomSheet extends StatelessWidget {
  final String title;
  final IconData titleIcon;
  final Widget child;
  final VoidCallback? onOk;
  final VoidCallback? onCancel;
  final bool showOkButton;
  final bool showCancelButton;
  final String? okText;
  final String? cancelText;

  const CommonBottomSheet({
    super.key,
    required this.title,
    required this.titleIcon,
    this.onOk,
    this.onCancel,
    required this.child,
    this.showOkButton = true,
    this.showCancelButton = true,
    this.okText,
    this.cancelText,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final s = S.of(context);
    return SingleChildScrollView(
      padding: MediaQuery.of(context).viewInsets,
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            ModalSheetSeparator(),
            BottomSheetTitle(icon: titleIcon, title: title),
            child,
            if (showOkButton || showCancelButton)
              ButtonBar(
                buttonPadding: const EdgeInsets.symmetric(horizontal: 10),
                children: <Widget>[
                  if (showCancelButton)
                    OutlinedButton(
                      onPressed: onCancel != null ? () => onCancel!() : () => _cancel(context),
                      child: Text(cancelText ?? s.cancel, style: TextStyle(color: theme.primaryColor)),
                    ),
                  if (showOkButton)
                    ElevatedButton(
                      onPressed: onOk != null ? () => onOk!() : null,
                      child: Text(okText ?? s.ok),
                    )
                ],
              )
          ],
        ),
      ),
    );
  }

  void _cancel(BuildContext context) {
    Navigator.of(context).pop();
  }
}
