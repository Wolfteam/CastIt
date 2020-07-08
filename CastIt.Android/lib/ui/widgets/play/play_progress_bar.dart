import 'package:castit/bloc/server_ws/server_ws_bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/play/play_bloc.dart';

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
            return Slider(
              onChanged: (double value) => _goToSeconds(context, value),
              value: state.currentSeconds,
              max: state.duration,
              activeColor: theme.accentColor,
            );
          },
        );
      },
    );
  }

  void _goToSeconds(BuildContext ctx, double seconds) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.gotoSeconds(seconds);
  }
}