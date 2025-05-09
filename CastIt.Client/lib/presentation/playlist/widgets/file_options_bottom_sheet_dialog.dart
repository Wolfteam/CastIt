import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlist/widgets/delete_file_bottom_sheet.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class FileOptionsBottomSheetDialog extends StatelessWidget {
  final int id;
  final int playListId;
  final String fileName;

  const FileOptionsBottomSheetDialog({
    required this.id,
    required this.playListId,
    required this.fileName,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    final textColor = theme.brightness == Brightness.dark ? Colors.white : Colors.black;
    return CommonBottomSheet(
      title: i18n.fileOptions,
      titleIcon: Icons.insert_drive_file,
      showOkButton: false,
      showCancelButton: false,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: <Widget>[
          TextButton(
            style: TextButton.styleFrom(foregroundColor: textColor),
            onPressed: () => _playFile(context, false),
            child: Row(
              children: <Widget>[
                const Icon(Icons.play_arrow),
                const SizedBox(width: 10),
                Text(i18n.playFile),
              ],
            ),
          ),
          TextButton(
            style: TextButton.styleFrom(foregroundColor: textColor),
            onPressed: () => _playFile(context, true),
            child: Row(
              children: <Widget>[
                const Icon(Icons.play_circle_filled),
                const SizedBox(width: 10),
                Text(i18n.playFileFromTheBeginning),
              ],
            ),
          ),
          TextButton(
            style: TextButton.styleFrom(foregroundColor: textColor),
            onPressed: () => _showDeleteModal(context),
            child: Row(
              children: <Widget>[
                const Icon(Icons.delete),
                const SizedBox(width: 10),
                Text(i18n.deleteFile),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _playFile(BuildContext context, bool force) {
    Navigator.of(context).pop(true);
    return context.read<ServerWsBloc>().playFile(id, playListId, force: force);
  }

  void _showDeleteModal(BuildContext context) {
    Navigator.of(context).pop(false);
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => DeleteFileBottomSheet(id: id, playListId: playListId, fileName: fileName),
    );
  }
}
