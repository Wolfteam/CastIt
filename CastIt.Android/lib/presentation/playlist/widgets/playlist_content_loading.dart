import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:flutter/material.dart';

class PlayListContentLoading extends StatelessWidget {
  const PlayListContentLoading({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: const [
        PlayListHeader(showSearch: false),
        Expanded(
          child: Center(
            child: CircularProgressIndicator(),
          ),
        ),
      ],
    );
  }
}
