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

  const FileItem({
    @required this.position,
    @required this.id,
    @required this.playListId,
    @required this.totalSeconds,
    @required this.isBeingPlayed,
    @required this.name,
    @required this.path,
    @required this.size,
    @required this.ext,
    @required this.playedPercentage,
    @required this.exists,
    @required this.isLocalFile,
    @required this.isUrlFile,
    @required this.loop,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final extraInfo = '$ext | $size';

    return Column(
      children: <Widget>[
        ListTile(
          isThreeLine: true,
          selected: loop,
          leading: ItemCounter(position),
          contentPadding: const EdgeInsets.symmetric(horizontal: 16),
          title: Text(
            name,
            overflow: TextOverflow.ellipsis,
            style: theme.textTheme.headline6,
          ),
          subtitle: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(
                path,
                overflow: TextOverflow.ellipsis,
              ),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: <Widget>[
                  Text(
                    extraInfo,
                    overflow: TextOverflow.ellipsis,
                  ),
                  Text(
                    totalSeconds > 0 ? Duration(seconds: totalSeconds.round()).formatDuration() : 'N/A',
                    overflow: TextOverflow.ellipsis,
                  ),
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
                    min: 0,
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
      ],
    );
  }

  void _playFile(BuildContext ctx) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.playFile(id, playListId);
    ctx.bloc<MainBloc>().add(MainEvent.goToTab(index: 0));
    Navigator.of(ctx).pop();
  }

  void _showFileOptionsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => const FileOptionsBottomSheetDialog(),
    );
  }
}