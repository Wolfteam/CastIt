import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/main/main_bloc.dart';
import '../../../bloc/server_ws/server_ws_bloc.dart';
import '../../../common/extensions/duration_extensions.dart';
import '../../../common/styles.dart';
import '../modals/file_options_bottom_sheet_dialog.dart';
import 'item_counter.dart';

class FileItem extends StatelessWidget {
  final int position;
  final int id;
  final int playListId;
  final double totalSeconds;
  final bool isBeingPlayed;
  final String name;
  final String path;
  final String size;
  final String ext;
  final double playedPercentage;
  final bool isLocalFile;
  final bool isUrlFile;
  final bool exists;
  final bool loop;
  final double itemHeight;
  final String subtitle;
  final double playedSeconds;

  const FileItem({
    required this.position,
    required this.id,
    required this.playListId,
    required this.totalSeconds,
    required this.isBeingPlayed,
    required this.name,
    required this.path,
    required this.size,
    required this.ext,
    required this.playedPercentage,
    required this.exists,
    required this.isLocalFile,
    required this.isUrlFile,
    required this.loop,
    required this.itemHeight,
    required this.subtitle,
    required this.playedSeconds,
    Key? key,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final title = !loop
        ? Text(
            name,
            overflow: TextOverflow.ellipsis,
            style: theme.textTheme.headline6,
          )
        : Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: <Widget>[
              Flexible(
                flex: 90,
                fit: FlexFit.tight,
                child: Text(
                  name,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.headline6,
                ),
              ),
              const Flexible(
                flex: 10,
                fit: FlexFit.tight,
                child: Icon(Icons.loop, size: 20),
              ),
            ],
          );

    final playedDurationText = playedSeconds >= 0 ? Duration(seconds: playedSeconds.round()).formatDuration() : 'N/A';
    final totalDurationText = totalSeconds > 0 ? Duration(seconds: totalSeconds.round()).formatDuration() : 'N/A';
    return Container(
      color: isBeingPlayed ? theme.accentColor.withOpacity(0.5) : null,
      height: itemHeight,
      child: ListTile(
        isThreeLine: true,
        selected: loop,
        leading: ItemCounter(position),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16),
        title: title,
        subtitle: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: <Widget>[
            Text(path, overflow: TextOverflow.ellipsis),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: <Widget>[
                Text(subtitle, overflow: TextOverflow.ellipsis),
                Text('$playedDurationText / $totalDurationText', overflow: TextOverflow.ellipsis),
              ],
            ),
            Container(
              margin: const EdgeInsets.only(top: 5),
              child: SliderTheme(
                data: SliderTheme.of(context).copyWith(
                  trackHeight: 1,
                  minThumbSeparation: 0,
                  disabledActiveTrackColor: theme.accentColor,
                  overlayShape: const RoundSliderThumbShape(enabledThumbRadius: 0.0, disabledThumbRadius: 0.0),
                  thumbColor: Colors.transparent,
                  thumbShape: const RoundSliderThumbShape(enabledThumbRadius: 0.0, disabledThumbRadius: 0.0),
                ),
                child: Slider(
                  value: playedPercentage,
                  max: 100,
                  activeColor: Colors.black,
                  inactiveColor: Colors.grey,
                  onChanged: null,
                ),
              ),
            ),
          ],
        ),
        dense: true,
        onTap: () => _playFile(context),
        onLongPress: () => _showFileOptionsModal(context),
      ),
    );
  }

  void _playFile(BuildContext ctx) {
    final bloc = ctx.read<ServerWsBloc>();
    bloc.playFile(id, playListId);
    _goToMainPage(ctx);
  }

  void _goToMainPage(BuildContext ctx) {
    ctx.read<MainBloc>().add(MainEvent.goToTab(index: 0));
    Navigator.of(ctx).pop();
  }

  Future<void> _showFileOptionsModal(BuildContext context) async {
    final closePage = await showModalBottomSheet<bool>(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => FileOptionsBottomSheetDialog(id: id, playListId: playListId, fileName: name),
    );

    if (closePage == true) {
      _goToMainPage(context);
    }
  }
}
