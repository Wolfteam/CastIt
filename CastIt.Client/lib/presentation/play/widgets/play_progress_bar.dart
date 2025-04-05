import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/duration_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayProgressBar extends StatelessWidget {
  final double? duration;
  final double? currentSeconds;

  const PlayProgressBar({this.duration, this.currentSeconds});

  @override
  Widget build(BuildContext context) {
    if (duration == null) {
      return const _DummySlider();
    }

    if (duration! <= 0) {
      return const _DummySlider(value: 100);
    }

    return Slider(
      value: currentSeconds ?? 0,
      max: duration!,
      label: _generateLabel(currentSeconds ?? 0),
      divisions: duration!.round(),
      onChangeStart: (value) => context.read<PlayBloc>().add(const PlayEvent.sliderDragChanged(isSliding: true)),
      onChanged: (value) => context.read<PlayBloc>().add(PlayEvent.sliderValueChanged(newValue: value)),
      onChangeEnd:
          (value) => context.read<PlayBloc>().add(
            PlayEvent.sliderValueChanged(newValue: value.roundToDouble(), triggerGoToSeconds: true),
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
    return Slider(onChanged: null, value: value, max: 100);
  }
}
