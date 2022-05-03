import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/duration_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayProgressText extends StatelessWidget {
  const PlayProgressText();

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<PlayBloc, PlayState>(
      builder: (ctx, state) {
        final theme = Theme.of(context);
        final isDarkTheme = theme.brightness == Brightness.dark;
        return state.maybeMap(
          playing: (state) {
            final current = Duration(seconds: (state.currentSeconds ?? 0).round()).formatDuration();
            final total = Duration(seconds: state.duration!.round()).formatDuration();
            return Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: <Widget>[
                  Text(
                    current,
                    style: TextStyle(color: isDarkTheme ? Colors.white : Colors.black),
                  ),
                  Text(
                    total,
                    style: TextStyle(color: isDarkTheme ? Colors.white : Colors.black),
                  )
                ],
              ),
            );
          },
          orElse: () => const Padding(
            padding: EdgeInsets.symmetric(horizontal: 16.0),
          ),
        );
      },
    );
  }
}
