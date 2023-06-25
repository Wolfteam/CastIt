import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:flutter/material.dart';

class PlayListContentNotFound extends StatelessWidget {
  const PlayListContentNotFound({super.key});

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final theme = Theme.of(context);
    return Column(
      children: [
        const PlayListHeader(showSearch: false),
        Expanded(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: <Widget>[
              const Icon(Icons.info_outline, size: 60),
              Text(
                s.playlistNotFound,
                textAlign: TextAlign.center,
                style: theme.textTheme.headlineMedium,
              ),
            ],
          ),
        )
      ],
    );
  }
}
