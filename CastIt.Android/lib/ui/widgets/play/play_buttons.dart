import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/play/play_bloc.dart';

class PlayButtons extends StatelessWidget {
  final bool areDisabled;

  const PlayButtons({
    Key key,
    this.areDisabled = false,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDarkTheme = theme.brightness == Brightness.dark;
    return Container(
      margin: const EdgeInsets.only(bottom: 50),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: <Widget>[
          IconButton(
            iconSize: 42,
            onPressed: areDisabled ? null : () {},
            icon: Icon(
              Icons.fast_rewind,
              color: isDarkTheme ? Colors.white : Colors.black,
            ),
          ),
          IconButton(
            iconSize: 42,
            onPressed: areDisabled ? null : () {},
            icon: Icon(
              Icons.skip_previous,
              color: isDarkTheme ? Colors.white : Colors.black,
            ),
          ),
          const SizedBox(width: 10.0),
          Container(
            decoration: BoxDecoration(
              color: theme.accentColor,
              borderRadius: BorderRadius.circular(50.0),
            ),
            child: BlocBuilder<PlayBloc, PlayState>(
              builder: (ctx, state) {
                if (state is PlayingState) {
                  return _buildPlayBackButton(state.isPaused);
                }
                return _buildPlayBackButton(false);
              },
            ),
          ),
          const SizedBox(width: 10.0),
          IconButton(
            iconSize: 42,
            onPressed: areDisabled ? null : () {},
            icon: Icon(
              Icons.skip_next,
              color: isDarkTheme ? Colors.white : Colors.black,
            ),
          ),
          IconButton(
            iconSize: 42,
            onPressed: areDisabled ? null : () {},
            icon: Icon(
              Icons.fast_forward,
              color: isDarkTheme ? Colors.white : Colors.black,
            ),
          ),
        ],
      ),
    );
  }

  IconButton _buildPlayBackButton(bool isPaused) {
    return IconButton(
      iconSize: 60,
      onPressed: areDisabled ? null : () {},
      icon: Icon(
        isPaused ? Icons.pause : Icons.play_arrow,
        color: Colors.white,
      ),
    );
  }
}
