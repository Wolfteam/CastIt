import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/play/play_bloc.dart';
import '../../../bloc/server_ws/server_ws_bloc.dart';

class PlayButtons extends StatelessWidget {
  static const double _iconSize = 42;

  final bool areDisabled;

  const PlayButtons({
    Key key,
    this.areDisabled = false,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDarkTheme = theme.brightness == Brightness.dark;
    final iconColor = isDarkTheme ? Colors.white : Colors.black;
    return Container(
      margin: const EdgeInsets.only(bottom: 50, left: 10, right: 10),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: <Widget>[
          IconButton(
            iconSize: _iconSize,
            onPressed: areDisabled ? null : () => _skipThirtySeconds(context, false),
            icon: Icon(Icons.fast_rewind, color: iconColor),
          ),
          IconButton(
            iconSize: _iconSize,
            onPressed: areDisabled ? null : () => _goTo(context, false, true),
            icon: Icon(Icons.skip_previous, color: iconColor),
          ),
          Container(
            decoration: BoxDecoration(
              color: theme.accentColor,
              borderRadius: BorderRadius.circular(50.0),
            ),
            child: BlocBuilder<PlayBloc, PlayState>(
              builder: (ctx, state) {
                if (state is PlayingState) {
                  return _buildPlayBackButton(context, state.isPaused);
                }
                return _buildPlayBackButton(context, false);
              },
            ),
          ),
          IconButton(
            iconSize: _iconSize,
            onPressed: areDisabled ? null : () => _goTo(context, true, false),
            icon: Icon(Icons.skip_next, color: iconColor),
          ),
          IconButton(
            iconSize: _iconSize,
            onPressed: areDisabled ? null : () => _skipThirtySeconds(context, true),
            icon: Icon(Icons.fast_forward, color: iconColor),
          ),
        ],
      ),
    );
  }

  IconButton _buildPlayBackButton(BuildContext ctx, bool isPaused) {
    return IconButton(
      iconSize: 60,
      onPressed: areDisabled ? null : () => _togglePlayBack(ctx),
      icon: Icon(
        isPaused ? Icons.pause : Icons.play_arrow,
        color: Colors.white,
      ),
    );
  }

  void _togglePlayBack(BuildContext ctx) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.togglePlayBack();
  }

  void _goTo(BuildContext ctx, bool next, bool previous) {
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.goTo(next: next, previous: previous);
  }

  void _skipThirtySeconds(BuildContext ctx, bool forward) {
    final seconds = 30.0 * (forward ? 1 : -1);
    final bloc = ctx.bloc<ServerWsBloc>();
    bloc.skipSeconds(seconds);
  }
}
