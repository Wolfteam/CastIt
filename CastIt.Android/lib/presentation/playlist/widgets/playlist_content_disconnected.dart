import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:castit/presentation/shared/something_went_wrong.dart';
import 'package:flutter/material.dart';

class PlayListContentDisconnected extends StatelessWidget {
  const PlayListContentDisconnected({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: const [
        PlayListHeader(showSearch: false),
        SomethingWentWrong(),
      ],
    );
  }
}
