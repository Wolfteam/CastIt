import 'package:castit/bloc/main/main_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/server_ws/server_ws_bloc.dart';
import 'item_counter.dart';

class FileItem extends StatelessWidget {
  final int _index;
  final int _id;
  final int _playListId;
  final String _filename;
  final String _filePath;
  final String _fileSize;
  final String _fileExt;

  const FileItem(
    this._index,
    this._id,
    this._playListId,
    this._filename,
    this._filePath,
    this._fileSize,
    this._fileExt,
  );

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final extraInfo = '$_fileExt | $_fileSize';
    return ListTile(
      isThreeLine: true,
      onLongPress: () {},
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 5),
      title: Text(
        _filename,
        overflow: TextOverflow.ellipsis,
        style: theme.textTheme.headline6,
      ),
      subtitle: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Text(
            _filePath,
            overflow: TextOverflow.ellipsis,
          ),
          Text(
            extraInfo,
            overflow: TextOverflow.ellipsis,
          ),
        ],
      ),
      dense: true,
      onTap: () => _playFile(context),
      leading: ItemCounter(_index),
    );
  }

  void _playFile(BuildContext ctx) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.playFile(_id, _playListId);
    ctx.bloc<MainBloc>().add(MainEvent.goToTab(index: 0));
    Navigator.of(ctx).pop();
  }
}
