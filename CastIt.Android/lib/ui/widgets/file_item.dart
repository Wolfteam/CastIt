import 'package:castit/ui/widgets/item_counter.dart';
import 'package:flutter/material.dart';

class FileItem extends StatelessWidget {
  final int _index;
  final String _filename;
  final String _filePath;
  final String _fileSize;
  final String _fileExt;

  const FileItem(
    this._index,
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
      contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 5),
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
      onTap: () {},
      leading: ItemCounter(_index),
    );
  }
}
