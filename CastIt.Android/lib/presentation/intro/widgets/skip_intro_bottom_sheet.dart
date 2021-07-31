import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/confirm_bottom_sheet.dart';
import 'package:flutter/material.dart';

class SkipIntroBottomSheet extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return ConfirmBottomSheet(
      title: i18n.confirm,
      icon: Icons.skip_next,
      content: i18n.skipIntroConfirm,
      onOk: () => _onCancel(context, skipped: true),
      onCancel: () => _onCancel(context),
    );
  }

  void _onCancel(BuildContext context, {bool skipped = false}) => Navigator.of(context).pop(skipped);
}
