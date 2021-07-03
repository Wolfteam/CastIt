import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'file_options_bottom_sheet_dialog.dart';
import 'item_counter.dart';

class FileItem extends StatelessWidget {
  final int position;
  final int id;
  final int playListId;
  final double totalSeconds;
  final bool isBeingPlayed;
  final String name;
  final String path;
  final double playedPercentage;
  final bool isLocalFile;
  final bool isUrlFile;
  final bool exists;
  final bool loop;
  final double itemHeight;
  final String subtitle;
  final double playedSeconds;
  final String fullTotalDuration;

  FileItem.fromItem({
    required this.itemHeight,
    required FileItemResponseDto file,
    Key? key,
  })  : id = file.id,
        position = file.position,
        playListId = file.playListId,
        isBeingPlayed = file.isBeingPlayed,
        totalSeconds = file.totalSeconds,
        name = file.filename,
        path = file.path,
        exists = file.exists,
        isLocalFile = file.isLocalFile,
        isUrlFile = file.isUrlFile,
        playedPercentage = file.playedPercentage,
        loop = file.loop,
        subtitle = file.subTitle,
        playedSeconds = file.playedSeconds,
        fullTotalDuration = file.fullTotalDuration,
        super(key: key);

  @override
  Widget build(BuildContext context) {
    // print('Building item = $id');
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
                BlocBuilder<PlayedFileItemBloc, PlayedFileItemState>(
                  builder: (ctx, state) => Text(
                    state.maybeMap(
                      playing: (state) => state.id == id && state.playListId == playListId ? state.fullTotalDuration : fullTotalDuration,
                      orElse: () => fullTotalDuration,
                    ),
                    overflow: TextOverflow.ellipsis,
                  ),
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
                child: BlocBuilder<PlayedFileItemBloc, PlayedFileItemState>(
                  builder: (ctx, state) => Slider(
                    value: state.maybeMap(
                      playing: (state) => state.id == id && state.playListId == playListId ? state.playedPercentage : playedPercentage,
                      orElse: () => playedPercentage,
                    ),
                    max: 100,
                    activeColor: Colors.black,
                    inactiveColor: Colors.grey,
                    onChanged: null,
                  ),
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
