import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/datetime_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
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
  final DateTime? lastPlayedDate;

  FileItem.fromItem({super.key, required this.itemHeight, required FileItemResponseDto file})
    : id = file.id,
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
      lastPlayedDate = file.lastPlayedDate;

  @override
  State<FileItem> createState() => _FileItemState();
}

class _FileItemState extends State<FileItem> {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    const horizontalPadding = EdgeInsets.symmetric(horizontal: 16);
    return InkWell(
      onTap: () => _playFile(),
      onLongPress: () => _showFileOptionsModal(),
      child: Container(
        color: widget.isBeingPlayed ? theme.colorScheme.secondaryContainer : null,
        height: widget.itemHeight,
        padding: horizontalPadding,
        child: Row(
          children: [
            ItemCounter(widget.position),
            Expanded(
              child: _Content(
                id: widget.id,
                playListId: widget.playListId,
                fullTotalDuration: widget.fullTotalDuration,
                name: widget.name,
                loop: widget.loop,
                path: widget.path,
                subtitle: widget.subtitle,
                playedPercentage: widget.playedPercentage,
                lastPlayedDate: widget.lastPlayedDate,
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _playFile() {
    final bloc = context.read<ServerWsBloc>();
    bloc.playFile(widget.id, widget.playListId);
    _goToMainPage();
  }

  void _goToMainPage() {
    context.read<MainBloc>().add(const MainEvent.goToTab(index: 0));
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
      return Text(name, overflow: TextOverflow.ellipsis, style: theme.textTheme.titleLarge);
    }
    return Row(
      children: <Widget>[
        Text(name, overflow: TextOverflow.ellipsis, style: theme.textTheme.titleLarge),
        const Icon(Icons.loop, size: 20),
      ],
    );
  }
}

class _PlayedSlider extends StatelessWidget {
  final int id;
  final int playListId;
  final double playedPercentage;

  const _PlayedSlider({
    required this.id,
    required this.playListId,
    required this.playedPercentage,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final s = S.of(context);
    return SliderTheme(
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
          value: switch (state) {
            PlayedFileItemStateNotPlayingState() => playedPercentage,
            PlayedFileItemStateLoadedState() =>
              state.id == id && state.playListId == playListId ? state.playedPercentage : playedPercentage,
          },
          max: 100,
          activeColor: Colors.black,
          inactiveColor: Colors.grey,
          onChanged: null,
        ),
      ),
    );
  }
}

class _PlayedTime extends StatelessWidget {
  final int id;
  final int playListId;
  final String fullTotalDuration;

  const _PlayedTime({
    required this.id,
    required this.playListId,
    required this.fullTotalDuration,
  });

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<PlayedFileItemBloc, PlayedFileItemState>(
      builder: (ctx, state) => Text(
        switch (state) {
          PlayedFileItemStateNotPlayingState() => fullTotalDuration,
          PlayedFileItemStateLoadedState() =>
            state.id == id && state.playListId == playListId ? state.fullTotalDuration : fullTotalDuration,
        },
        overflow: TextOverflow.ellipsis,
      ),
    );
  }
}

class _Content extends StatelessWidget {
  final int id;
  final int playListId;
  final String fullTotalDuration;
  final String name;
  final bool loop;
  final String path;
  final String subtitle;
  final double playedPercentage;
  final DateTime? lastPlayedDate;

  const _Content({
    required this.id,
    required this.playListId,
    required this.fullTotalDuration,
    required this.name,
    required this.loop,
    required this.path,
    required this.subtitle,
    required this.playedPercentage,
    this.lastPlayedDate,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final s = S.of(context);
    const horizontalPadding = EdgeInsets.symmetric(horizontal: 8);
    return Padding(
      padding: horizontalPadding,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    _Title(name: name, loop: loop),
                    Text(
                      path,
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.bodySmall!.copyWith(fontWeight: FontWeight.normal),
                    ),
                    Text(
                      subtitle,
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.bodySmall!.copyWith(fontWeight: FontWeight.normal),
                    ),
                    Text(
                      s.lastPlayedDate(lastPlayedDate != null ? lastPlayedDate.formatLastPlayedDate()! : s.na),
                      overflow: TextOverflow.ellipsis,
                      style: theme.textTheme.labelSmall!.copyWith(fontWeight: FontWeight.normal),
                    ),
                  ],
                ),
              ),
              Container(
                margin: const EdgeInsets.only(left: 16),
                child: _PlayedTime(
                  id: id,
                  playListId: playListId,
                  fullTotalDuration: fullTotalDuration,
                ),
              ),
            ],
          ),
          _PlayedSlider(
            id: id,
            playListId: playListId,
            playedPercentage: playedPercentage,
          ),
        ],
      ),
    );
  }
}
