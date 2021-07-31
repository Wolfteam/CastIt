import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../shared/confirm_bottom_sheet.dart';

class DeleteFileBottomSheet extends StatelessWidget {
  final int id;
  final int playListId;
  final String fileName;

  const DeleteFileBottomSheet({
    Key? key,
    required this.id,
    required this.playListId,
    required this.fileName,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return ConfirmBottomSheet(
      title: i18n.delete,
      icon: Icons.delete,
      content: i18n.deleteFileConfirm(fileName),
      onOk: () => _onOk(context),
      onCancel: () => _onCancel(context),
    );
  }

  Future<void> _onOk(BuildContext context) async {
    await context.read<ServerWsBloc>().deleteFile(id, playListId);
    _onCancel(context);
  }

  void _onCancel(BuildContext context) => Navigator.of(context).pop();
}
