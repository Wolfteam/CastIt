import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:castit/presentation/shared/something_went_wrong.dart';
import 'package:flutter/material.dart';

class PlayListContentDisconnected extends StatelessWidget {
  const PlayListContentDisconnected({super.key});

  @override
  Widget build(BuildContext context) {
    return const Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PlayListHeader(showSearch: false),
        SomethingWentWrong(),
      ],
    );
  }
}
