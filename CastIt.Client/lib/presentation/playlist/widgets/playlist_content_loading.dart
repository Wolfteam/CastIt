import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:flutter/material.dart';

class PlayListContentLoading extends StatelessWidget {
  const PlayListContentLoading({super.key});

  @override
  Widget build(BuildContext context) {
    return const Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
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
