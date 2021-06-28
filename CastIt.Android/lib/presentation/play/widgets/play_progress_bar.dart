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
        final dummySlider = Slider(
          onChanged: null,
          value: 0,
          max: 100,
          activeColor: theme.accentColor,
        );
        return state.map(
          connecting: (state) => dummySlider,
          connected: (state) => dummySlider,
          fileLoading: (state) => dummySlider,
          fileLoadingFailed: (state) => dummySlider,
          playing: (state) {
            final theme = Theme.of(context);
            if (state.duration! <= 0) {
              return Slider(
                onChanged: null,
                value: 100,
                max: 100,
                activeColor: theme.accentColor,
              );
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
                    context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: finalValue.roundToDouble(), triggerGoToSeconds: true)));
          },
        );
      },
    );
  }

  String _generateLabel(double seconds) => Duration(seconds: seconds.round()).formatDuration();
}
