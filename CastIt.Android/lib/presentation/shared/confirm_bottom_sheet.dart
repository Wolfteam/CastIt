import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:flutter/material.dart';

import 'common_bottom_sheet.dart';

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
    final theme = Theme.of(context);
    return CommonBottomSheet(
      title: title,
      titleIcon: icon,
      onOk: onOk,
      onCancel: onCancel,
      okText: okText,
      cancelText: cancelText,
      child: Padding(
        padding: const EdgeInsets.only(top: 10, left: 10),
        child: Text(content, style: theme.textTheme.subtitle1),
      ),
    );
  }
}
