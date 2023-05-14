import 'package:castit/application/bloc.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:responsive_builder/responsive_builder.dart';

class PlayButtons extends StatelessWidget {
  static const double _iconSizeOnMobile = 50;
  static const double _iconSizeOnDesktopOrTablet = 70;
  static const double _playbackIconSizeOnMobile = 65;
  static const double _playbackIconSizeDesktopOrTablet = 85;

  final bool areDisabled;

  const PlayButtons({this.areDisabled = false});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDarkTheme = theme.brightness == Brightness.dark;
    final iconColor = isDarkTheme ? Colors.white : Colors.black;
    return ResponsiveBuilder(
      builder: (ctx, sizing) {
        final isDesktopOrTable = sizing.isDesktop || sizing.isTablet;
        final iconSize = isDesktopOrTable ? _iconSizeOnDesktopOrTablet : _iconSizeOnMobile;
        final playBackIconSize = isDesktopOrTable ? _playbackIconSizeDesktopOrTablet : _playbackIconSizeOnMobile;
        return Padding(
          padding: EdgeInsets.symmetric(horizontal: isDesktopOrTable ? 0 : 15),
          child: Row(
            mainAxisAlignment: isDesktopOrTable ? MainAxisAlignment.spaceEvenly : MainAxisAlignment.spaceBetween,
            children: <Widget>[
              IconButton(
                iconSize: iconSize,
                padding: EdgeInsets.zero,
                onPressed: areDisabled ? null : () => _skipThirtySeconds(context, false),
                icon: Icon(Icons.fast_rewind, color: iconColor),
              ),
              IconButton(
                iconSize: iconSize,
                padding: EdgeInsets.zero,
                onPressed: areDisabled ? null : () => _goTo(context, false, true),
                icon: Icon(Icons.skip_previous, color: iconColor),
              ),
              DecoratedBox(
                decoration: BoxDecoration(
                  color: theme.colorScheme.secondary,
                  borderRadius: BorderRadius.circular(50.0),
                ),
                child: BlocBuilder<PlayBloc, PlayState>(
                  builder: (ctx, state) => state.maybeMap(
                    playing: (state) => _PlayBackButton(iconSize: playBackIconSize, isPaused: state.isPaused!, isDisabled: areDisabled),
                    orElse: () => _PlayBackButton(iconSize: playBackIconSize, isPaused: false, isDisabled: areDisabled),
                  ),
                ),
              ),
              IconButton(
                iconSize: iconSize,
                padding: EdgeInsets.zero,
                onPressed: areDisabled ? null : () => _goTo(context, true, false),
                icon: Icon(Icons.skip_next, color: iconColor),
              ),
              IconButton(
                iconSize: iconSize,
                padding: EdgeInsets.zero,
                onPressed: areDisabled ? null : () => _skipThirtySeconds(context, true),
                icon: Icon(Icons.fast_forward, color: iconColor),
              ),
            ],
          ),
        );
      },
    );
  }

  void _goTo(BuildContext ctx, bool next, bool previous) {
    final bloc = ctx.read<ServerWsBloc>();
    bloc.goTo(next: next, previous: previous);
  }

  void _skipThirtySeconds(BuildContext ctx, bool forward) {
    final seconds = 30.0 * (forward ? 1 : -1);
    final bloc = ctx.read<ServerWsBloc>();
    bloc.skipSeconds(seconds);
  }
}

class _PlayBackButton extends StatelessWidget {
  final double iconSize;
  final bool isDisabled;
  final bool isPaused;

  const _PlayBackButton({
    required this.iconSize,
    required this.isDisabled,
    required this.isPaused,
  });

  @override
  Widget build(BuildContext context) {
    return IconButton(
      iconSize: iconSize,
      onPressed: isDisabled ? null : () => _togglePlayBack(context),
      icon: Icon(
        isPaused ? Icons.pause : Icons.play_arrow,
        color: Colors.white,
      ),
    );
  }

  void _togglePlayBack(BuildContext ctx) {
    final bloc = ctx.read<ServerWsBloc>();
    bloc.togglePlayBack();
  }
}
