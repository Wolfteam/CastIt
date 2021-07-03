import 'package:castit/application/bloc.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayListFab extends StatelessWidget {
  final int id;
  final String name;
  final bool loop;
  final bool shuffle;
  final Function onArrowTopTap;
  final AnimationController hideFabAnimController;

  const PlayListFab({
    Key? key,
    required this.id,
    required this.name,
    required this.loop,
    required this.shuffle,
    required this.onArrowTopTap,
    required this.hideFabAnimController,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    const iconSize = 30.0;
    return FadeTransition(
      opacity: hideFabAnimController,
      child: Card(
        elevation: 10,
        shape: Styles.floatingCardShape,
        margin: const EdgeInsets.symmetric(horizontal: 20),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 10),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: <Widget>[
              ButtonBar(
                buttonPadding: const EdgeInsets.all(0),
                children: <Widget>[
                  IconButton(
                    icon: Icon(Icons.loop, color: loop ? theme.accentColor : null, size: iconSize),
                    onPressed: () => _setPlayListOptions(!loop, shuffle, context),
                  ),
                  IconButton(
                    icon: Icon(Icons.shuffle, color: shuffle ? theme.accentColor : null, size: iconSize),
                    onPressed: () => _setPlayListOptions(loop, !shuffle, context),
                  ),
                ],
              ),
              Expanded(
                child: Text(name, textAlign: TextAlign.center, overflow: TextOverflow.ellipsis),
              ),
              ButtonBar(
                buttonPadding: const EdgeInsets.all(0),
                children: <Widget>[
                  IconButton(
                    icon: const Icon(Icons.search, size: iconSize),
                    onPressed: () => _toggleSearchBoxVisibility(context),
                  ),
                  IconButton(
                    icon: const Icon(Icons.arrow_upward, size: iconSize),
                    onPressed: () => onArrowTopTap(),
                  ),
                ],
              )
            ],
          ),
        ),
      ),
    );
  }

  void _setPlayListOptions(bool loop, bool shuffle, BuildContext context) {
    final bloc = context.read<ServerWsBloc>();
    bloc.setPlayListOptions(id, loop: loop, shuffle: shuffle);
    context.read<PlayListBloc>().add(PlayListEvent.playListOptionsChanged(loop: loop, shuffle: shuffle));
  }

  void _toggleSearchBoxVisibility(BuildContext context) => context.read<PlayListBloc>().add(const PlayListEvent.toggleSearchBoxVisibility());
}
