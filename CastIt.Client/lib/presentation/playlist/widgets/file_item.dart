import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/datetime_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlist/widgets/file_options_bottom_sheet_dialog.dart';
import 'package:castit/presentation/playlist/widgets/item_counter.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
  final String playedTime;
  final String duration;
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
      playedTime = file.playedTime,
      duration = file.duration,
      fullTotalDuration = file.fullTotalDuration,
      lastPlayedDate = file.lastPlayedDate;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return InkWell(
      onTap: () => _playFile(context),
      onLongPress: () => _showFileOptionsModal(context),
      child: Container(
        color: isBeingPlayed ? theme.colorScheme.secondaryContainer : null,
        height: itemHeight,
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: Row(
          children: [
            ItemCounter(position),
            Expanded(
              child: _Content(
                id: id,
                playListId: playListId,
                fullTotalDuration: fullTotalDuration,
                name: name,
                loop: loop,
                path: path,
                subtitle: subtitle,
                playedPercentage: playedPercentage,
                playedTime: playedTime,
                duration: duration,
                lastPlayedDate: lastPlayedDate,
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _playFile(BuildContext context) {
    final bloc = context.read<ServerWsBloc>();
    bloc.playFile(id, playListId);
    _goToMainPage(context);
  }

  void _goToMainPage(BuildContext context) {
    context.read<MainBloc>().add(const MainEvent.goToTab(index: 0));
    Navigator.of(context).pop();
  }

  Future<void> _showFileOptionsModal(BuildContext context) async {
    final closePage = await showModalBottomSheet<bool>(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => FileOptionsBottomSheetDialog(id: id, playListId: playListId, fileName: name),
    );

    if (closePage == true && context.mounted) {
      _goToMainPage(context);
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
      return Text(name, overflow: TextOverflow.ellipsis, style: theme.textTheme.titleSmall);
    }
    return Row(
      children: <Widget>[
        Text(name, overflow: TextOverflow.ellipsis, style: theme.textTheme.titleSmall),
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
        trackHeight: 0.5,
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
  final String playedTime;
  final String duration;
  final String fullTotalDuration;
  final bool split;

  const _PlayedTime({
    required this.id,
    required this.playListId,
    required this.playedTime,
    required this.duration,
    required this.fullTotalDuration,
    this.split = false,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final textStyle = theme.textTheme.labelSmall!.copyWith(fontWeight: FontWeight.normal);
    return BlocBuilder<PlayedFileItemBloc, PlayedFileItemState>(
      builder: (ctx, state) {
        final List<String> parts = switch (state) {
          PlayedFileItemStateNotPlayingState() => !split ? [fullTotalDuration] : [playedTime, duration],
          final PlayedFileItemStateLoadedState state when state.id != id || state.playListId != playListId =>
            !split ? [fullTotalDuration] : [playedTime, duration],
          PlayedFileItemStateLoadedState() => !split ? [state.fullTotalDuration] : [state.playedTime, state.duration],
        };

        if (split && parts.length == 2) {
          return Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                parts.first,
                overflow: TextOverflow.ellipsis,
                style: textStyle,
              ),
              Text(
                parts.last,
                overflow: TextOverflow.ellipsis,
                style: textStyle,
              ),
            ],
          );
        }

        return Text(
          parts.first,
          overflow: TextOverflow.ellipsis,
          style: textStyle,
        );
      },
    );
  }
}

class _Content extends StatelessWidget {
  final int id;
  final int playListId;
  final String name;
  final bool loop;
  final String path;
  final String subtitle;
  final String playedTime;
  final String duration;
  final double playedPercentage;
  final String fullTotalDuration;
  final DateTime? lastPlayedDate;

  const _Content({
    required this.id,
    required this.playListId,
    required this.name,
    required this.loop,
    required this.path,
    required this.subtitle,
    required this.playedTime,
    required this.duration,
    required this.playedPercentage,
    required this.fullTotalDuration,
    this.lastPlayedDate,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final s = S.of(context);
    final isPortrait = MediaQuery.of(context).orientation == Orientation.portrait;
    const horizontalPadding = EdgeInsets.symmetric(horizontal: 8);
    final textStyle = theme.textTheme.labelSmall!.copyWith(fontWeight: FontWeight.normal);
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
                      style: textStyle,
                    ),
                    Text(
                      subtitle,
                      overflow: TextOverflow.ellipsis,
                      style: textStyle,
                    ),
                    Text(
                      s.lastPlayedDate(lastPlayedDate != null ? lastPlayedDate.formatLastPlayedDate()! : s.na),
                      overflow: TextOverflow.ellipsis,
                      style: textStyle,
                    ),
                  ],
                ),
              ),
              if (!isPortrait)
                Container(
                  margin: const EdgeInsets.only(left: 16),
                  child: _PlayedTime(
                    id: id,
                    playListId: playListId,
                    duration: duration,
                    playedTime: playedTime,
                    fullTotalDuration: fullTotalDuration,
                  ),
                ),
            ],
          ),
          if (isPortrait)
            _PlayedTime(
              id: id,
              playListId: playListId,
              duration: duration,
              playedTime: playedTime,
              fullTotalDuration: fullTotalDuration,
              split: true,
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
