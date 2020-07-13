import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/play/play_bloc.dart';
import '../../../common/extensions/duration_extensions.dart';

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
          playing: (state) => Slider(
            onChanged: (double value) =>
                context.bloc<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: value, triggerGoToSeconds: false)),
            value: state.currentSeconds,
            max: state.duration,
            activeColor: theme.accentColor,
            label: _generateLabel(state.currentSeconds),
            divisions: state.duration.round(),
            onChangeStart: (startValue) => context.bloc<PlayBloc>().add(PlayEvent.sliderDragChanged(isSliding: true)),
            onChangeEnd: (finalValue) => context
                .bloc<PlayBloc>()
                .add(PlayEvent.sliderValueChanged(newValue: finalValue.roundToDouble(), triggerGoToSeconds: true)),
          ),
        );
      },
    );
  }

  String _generateLabel(double seconds) => Duration(seconds: seconds.round()).formatDuration();
}
