import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/presentation/playlist/widgets/file_options_bottom_sheet_dialog.dart';
import 'package:castit/presentation/playlist/widgets/item_counter.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class FileItem extends StatefulWidget {
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
    super.key,
    required this.itemHeight,
    required FileItemResponseDto file,
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
        fullTotalDuration = file.fullTotalDuration;

  @override
  State<FileItem> createState() => _FileItemState();
}

class _FileItemState extends State<FileItem> {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      color: widget.isBeingPlayed ? theme.colorScheme.secondary.withOpacity(0.5) : null,
      height: widget.itemHeight,
      child: ListTile(
        isThreeLine: true,
        selected: widget.loop,
        leading: ItemCounter(widget.position),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16),
        title: _Title(name: widget.name, loop: widget.loop),
        subtitle: _Content(
          id: widget.id,
          playListId: widget.playListId,
          path: widget.path,
          subtitle: widget.subtitle,
          playedPercentage: widget.playedPercentage,
          fullTotalDuration: widget.fullTotalDuration,
        ),
        dense: true,
        onTap: () => _playFile(),
        onLongPress: () => _showFileOptionsModal(),
      ),
    );
  }

  void _playFile() {
    final bloc = context.read<ServerWsBloc>();
    bloc.playFile(widget.id, widget.playListId);
    _goToMainPage();
  }

  void _goToMainPage() {
    context.read<MainBloc>().add(MainEvent.goToTab(index: 0));
    Navigator.of(context).pop();
  }

  Future<void> _showFileOptionsModal() async {
    final closePage = await showModalBottomSheet<bool>(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => FileOptionsBottomSheetDialog(id: widget.id, playListId: widget.playListId, fileName: widget.name),
    );

    if (closePage == true) {
      _goToMainPage();
    }
  }
}

class _Title extends StatelessWidget {
  final String name;
  final bool loop;

  const _Title({required this.name, required this.loop});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    if (!loop) {
      return Text(
        name,
        overflow: TextOverflow.ellipsis,
        style: theme.textTheme.titleLarge,
      );
    }
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: <Widget>[
        Flexible(
          flex: 90,
          fit: FlexFit.tight,
          child: Text(
            name,
            overflow: TextOverflow.ellipsis,
            style: theme.textTheme.titleLarge,
          ),
        ),
        const Flexible(
          flex: 10,
          fit: FlexFit.tight,
          child: Icon(Icons.loop, size: 20),
        ),
      ],
    );
  }
}

class _Content extends StatelessWidget {
  final int id;
  final int playListId;
  final String path;
  final double playedPercentage;
  final String subtitle;
  final String fullTotalDuration;

  const _Content({
    required this.id,
    required this.playListId,
    required this.path,
    required this.playedPercentage,
    required this.subtitle,
    required this.fullTotalDuration,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Column(
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
              disabledActiveTrackColor: theme.colorScheme.secondary,
              overlayShape: const RoundSliderThumbShape(enabledThumbRadius: .1, disabledThumbRadius: .1),
              thumbColor: Colors.transparent,
              thumbShape: const RoundSliderThumbShape(enabledThumbRadius: .1, disabledThumbRadius: .1),
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
    );
  }
}
