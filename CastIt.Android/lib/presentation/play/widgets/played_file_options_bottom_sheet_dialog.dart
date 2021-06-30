import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import 'played_file_dropdown_option.dart';
import 'played_file_volume_option.dart';

class PlayedFileOptionsBottomSheetDialog extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    return CommonBottomSheet(
      title: s.fileOptions,
      titleIcon: Icons.play_circle_filled,
      showOkButton: false,
      showCancelButton: false,
      child: BlocConsumer<PlayedFileOptionsBloc, PlayedFileOptionsState>(
        listener: (ctx, state) => state.maybeWhen(
          closed: () {
            if (ModalRoute.of(context)!.isCurrent) {
              Navigator.of(ctx).pop();
            }
          },
          orElse: () {},
        ),
        builder: (ctx, state) => Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: state.map(
            loaded: (state) => <Widget>[
              PlayedFileDropdownOption(
                title: s.audio,
                hint: s.audio,
                icon: Icons.audiotrack,
                options: state.options.where((element) => element.isAudio).toList(),
              ),
              PlayedFileDropdownOption(
                title: s.subtitles,
                hint: s.subtitles,
                icon: Icons.subtitles,
                options: state.options.where((element) => element.isSubTitle).toList(),
              ),
              PlayedFileDropdownOption(
                title: s.quality,
                hint: s.quality,
                icon: Icons.high_quality,
                options: state.options.where((element) => element.isQuality).toList(),
              ),
              PlayedFileVolumeOption(
                volumeLevel: state.volumeLvl,
                isMuted: state.isMuted,
              ),
              OutlinedButton.icon(
                onPressed: () => _stopPlayback(context),
                icon: const Icon(Icons.stop),
                label: Text(s.stopPlayback),
              ),
            ],
            closed: (_) => [],
          ),
        ),
      ),
    );
  }

  Future<void> _stopPlayback(BuildContext context) async {
    await context.read<ServerWsBloc>().stopPlayBack();
    Navigator.of(context).pop();
  }
}
