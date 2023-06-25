import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/duration_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayProgressBar extends StatelessWidget {
  const PlayProgressBar();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<PlayBloc, PlayState>(
      builder: (ctx, state) => state.maybeMap(
        playing: (state) {
          if (state.duration! <= 0) {
            return const _DummySlider(value: 100);
          }
          return Slider(
            onChanged: (double value) => context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: value)),
            value: state.currentSeconds!,
            max: state.duration!,
            activeColor: Theme.of(context).colorScheme.secondary,
            label: _generateLabel(state.currentSeconds!),
            divisions: state.duration!.round(),
            onChangeStart: (startValue) => context.read<PlayBloc>().add(PlayEvent.sliderDragChanged(isSliding: true)),
            onChangeEnd: (finalValue) =>
                context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: finalValue.roundToDouble(), triggerGoToSeconds: true)),
          );
        },
        orElse: () => const _DummySlider(),
      ),
    );
  }

  String _generateLabel(double seconds) => Duration(seconds: seconds.round()).formatDuration();
}

class _DummySlider extends StatelessWidget {
  final double value;

  const _DummySlider({this.value = 0});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Slider(onChanged: null, value: value, max: 100, activeColor: theme.colorScheme.secondary);
  }
}
