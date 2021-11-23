import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/confirm_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DeletePlayListBottomSheet extends StatelessWidget {
  final int playListId;
  final String playListName;

  const DeletePlayListBottomSheet({
    required this.playListId,
    required this.playListName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return ConfirmBottomSheet(
      title: i18n.delete,
      icon: Icons.delete,
      content: i18n.deletePlayListConfirm(playListName),
      onOk: () => _onOk(context),
      onCancel: () => _onCancel(context),
    );
  }

  void _onOk(BuildContext context) {
    context.read<ServerWsBloc>().deletePlayList(playListId);
    _onCancel(context);
  }

  void _onCancel(BuildContext context) => Navigator.of(context).pop();
}
