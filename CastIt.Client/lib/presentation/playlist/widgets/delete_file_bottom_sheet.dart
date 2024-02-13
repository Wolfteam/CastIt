import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/confirm_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DeleteFileBottomSheet extends StatelessWidget {
  final int id;
  final int playListId;
  final String fileName;

  const DeleteFileBottomSheet({
    super.key,
    required this.id,
    required this.playListId,
    required this.fileName,
  });

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

  void _onOk(BuildContext context) {
    context.read<ServerWsBloc>().deleteFile(id, playListId);
    _onCancel(context);
  }

  void _onCancel(BuildContext context) => Navigator.of(context).pop();
}
