import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/play/play_bloc.dart';

class PlayProgressText extends StatelessWidget {
  const PlayProgressText();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<PlayBloc, PlayState>(
      builder: (ctx, state) {
        final theme = Theme.of(context);
        final isDarkTheme = theme.brightness == Brightness.dark;
        const dummy = Padding(
          padding: EdgeInsets.symmetric(horizontal: 16.0),
        );
        return state.map(
          connecting: (state) => dummy,
          connected: (state) => dummy,
          fileLoading: (state) => dummy,
          fileLoadingFailed: (state) => dummy,
          playing: (state) {
            final current = _formatDuration(Duration(
              seconds: (state.currentSeconds ?? 0).round(),
            ));

            final total = _formatDuration(Duration(
              seconds: state.duration.round(),
            ));
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: <Widget>[
                  Text(
                    current,
                    style: TextStyle(
                      color: isDarkTheme ? Colors.white : Colors.black,
                    ),
                  ),
                  Text(
                    total,
                    style: TextStyle(
                      color: isDarkTheme ? Colors.white : Colors.black,
                    ),
                  )
                ],
              ),
            );
          },
        );
      },
    );
  }

  String _formatDuration(Duration duration) {
    String twoDigits(num n) => n.toString().padLeft(2, '0');
    final twoDigitHour = twoDigits(duration.inHours);
    final twoDigitMinutes = twoDigits(duration.inMinutes.remainder(60));
    final twoDigitSeconds = twoDigits(duration.inSeconds.remainder(60));
    return '$twoDigitHour:$twoDigitMinutes:$twoDigitSeconds';
  }
}
