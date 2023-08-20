import 'package:castit/application/bloc.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

typedef OnTap = void Function();

class PlayListFab extends StatelessWidget {
  final int id;
  final String name;
  final bool loop;
  final bool shuffle;
  final OnTap onArrowTopTap;
  final bool isVisible;

  const PlayListFab({
    super.key,
    required this.id,
    required this.name,
    required this.loop,
    required this.shuffle,
    required this.onArrowTopTap,
    required this.isVisible,
  });

  @override
  Widget build(BuildContext context) {
    return IgnorePointer(
      ignoring: !isVisible,
      child: AnimatedOpacity(
        opacity: isVisible ? 1 : 0,
        duration: const Duration(milliseconds: 250),
        child: _CardRow(
          id: id,
          name: name,
          loop: loop,
          shuffle: shuffle,
          onArrowTopTap: onArrowTopTap,
        ),
      ),
    );
  }
}

class _CardRow extends StatelessWidget {
  final int id;
  final String name;
  final bool loop;
  final bool shuffle;
  final OnTap onArrowTopTap;

  const _CardRow({
    required this.id,
    required this.name,
    required this.loop,
    required this.shuffle,
    required this.onArrowTopTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    const iconSize = 30.0;
    return Container(
      constraints: const BoxConstraints(maxWidth: 600),
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
                children: <Widget>[
                  IconButton(
                    icon: Icon(Icons.loop, color: loop ? theme.colorScheme.secondary : null, size: iconSize),
                    onPressed: () => _setPlayListOptions(!loop, shuffle, context),
                    splashRadius: Styles.mediumButtonSplashRadius,
                  ),
                  IconButton(
                    icon: Icon(Icons.shuffle, color: shuffle ? theme.colorScheme.secondary : null, size: iconSize),
                    onPressed: () => _setPlayListOptions(loop, !shuffle, context),
                    splashRadius: Styles.mediumButtonSplashRadius,
                  ),
                ],
              ),
              Expanded(
                child: Text(name, textAlign: TextAlign.center, overflow: TextOverflow.ellipsis),
              ),
              ButtonBar(
                buttonPadding: EdgeInsets.zero,
                children: <Widget>[
                  IconButton(
                    icon: const Icon(Icons.search, size: iconSize),
                    onPressed: () => _toggleSearchBoxVisibility(context),
                    splashRadius: Styles.mediumButtonSplashRadius,
                  ),
                  IconButton(
                    icon: const Icon(Icons.arrow_upward, size: iconSize),
                    onPressed: () => onArrowTopTap(),
                    splashRadius: Styles.mediumButtonSplashRadius,
                  ),
                ],
              ),
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
