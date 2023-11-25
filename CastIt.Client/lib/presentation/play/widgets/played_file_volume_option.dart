import 'package:castit/application/bloc.dart';
import 'package:castit/domain/app_constants.dart';
import 'package:castit/generated/l10n.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayedFileVolumeOption extends StatelessWidget {
  final double volumeLevel;
  final bool isMuted;

  const PlayedFileVolumeOption({
    super.key,
    required this.volumeLevel,
    required this.isMuted,
  });

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
            children: <Widget>[
              const Icon(Icons.volume_up),
              Container(
                margin: const EdgeInsets.only(left: 10),
                child: Text(
                  i18n.volume,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.titleMedium,
                ),
              ),
            ],
          ),
          Row(
            children: <Widget>[
              Expanded(
                child: Slider(
                  value: volumeLevel,
                  max: AppConstants.maxVolumeLevel,
                  label: '${volumeLevel.round()}',
                  divisions: AppConstants.maxVolumeLevel.round(),
                  onChanged: (newValue) => _setVolume(context, newValue, isMuted, false),
                  onChangeStart: (startValue) => context.read<PlayedFileOptionsBloc>().add(PlayedFileOptionsEvent.volumeSliderDragStarted()),
                  onChangeEnd: (finalValue) => _setVolume(context, finalValue, isMuted, true),
                ),
              ),
              IconButton(
                icon: Icon(isMuted ? Icons.volume_off : Icons.volume_up),
                onPressed: () => _setVolume(context, volumeLevel, !isMuted, true),
              ),
            ],
          ),
        ],
      ),
    );
  }

  void _setVolume(BuildContext context, double volumeLevel, bool isMuted, bool triggerChange) => context
      .read<PlayedFileOptionsBloc>()
      .add(PlayedFileOptionsEvent.setVolume(volumeLvl: volumeLevel, isMuted: isMuted, triggerChange: triggerChange));
}
