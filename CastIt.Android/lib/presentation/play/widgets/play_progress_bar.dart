import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/duration_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayProgressBar extends StatelessWidget {
  const PlayProgressBar();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<PlayBloc, PlayState>(
      builder: (ctx, state) {
        final theme = Theme.of(context);
        return state.map(
          connecting: (_) => const _DummySlider(),
          connected: (_) => const _DummySlider(),
          fileLoading: (_) => const _DummySlider(),
          fileLoadingFailed: (_) => const _DummySlider(),
          playing: (state) {
            if (state.duration! <= 0) {
              return const _DummySlider(value: 100);
            }
            return Slider(
              onChanged: (double value) => context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: value, triggerGoToSeconds: false)),
              value: state.currentSeconds!,
              max: state.duration!,
              activeColor: theme.accentColor,
              label: _generateLabel(state.currentSeconds!),
              divisions: state.duration!.round(),
              onChangeStart: (startValue) => context.read<PlayBloc>().add(PlayEvent.sliderDragChanged(isSliding: true)),
              onChangeEnd: (finalValue) =>
                  context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: finalValue.roundToDouble(), triggerGoToSeconds: true)),
            );
          },
        );
      },
    );
  }

  String _generateLabel(double seconds) => Duration(seconds: seconds.round()).formatDuration();
}

class _DummySlider extends StatelessWidget {
  final double value;
  final double max;

  const _DummySlider({
    Key? key,
    this.value = 0,
    this.max = 100,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Slider(onChanged: null, value: value, max: 100, activeColor: theme.accentColor);
  }
}
