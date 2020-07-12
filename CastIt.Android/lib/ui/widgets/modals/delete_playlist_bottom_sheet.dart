import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/server_ws/server_ws_bloc.dart';
import '../../../generated/i18n.dart';
import 'confirm_bottom_sheet.dart';

class DeletePlayListBottomSheet extends StatelessWidget {
  final int playListId;
  final String playListName;

  const DeletePlayListBottomSheet({
    @required this.playListId,
    @required this.playListName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = I18n.of(context);
    return ConfirmBottomSheet(
      title: i18n.delete,
      icon: Icons.delete,
      content: i18n.deletePlayListConfirm(playListName),
      onOk: () => _onOk(context),
      onCancel: () => _onCancel(context),
    );
  }

  Future<void> _onOk(BuildContext context) async {
    await context.bloc<ServerWsBloc>().deletePlayList(playListId);
    _onCancel(context);
  }

  void _onCancel(BuildContext context) => Navigator.of(context).pop();
}
