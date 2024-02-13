import 'package:castit/domain/extensions/duration_extensions.dart';
import 'package:flutter/material.dart';

class PlayProgressText extends StatelessWidget {
  final double? currentSeconds;
  final double? duration;

  const PlayProgressText({
    required this.currentSeconds,
    required this.duration,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final isDarkTheme = theme.brightness == Brightness.dark;
    final current = Duration(seconds: (currentSeconds ?? 0).round()).formatDuration();
    final total = Duration(seconds: (duration ?? 0).round()).formatDuration();
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: <Widget>[
          Text(
            current,
            style: TextStyle(color: isDarkTheme ? Colors.white : Colors.black),
          ),
          Text(
            total,
            style: TextStyle(color: isDarkTheme ? Colors.white : Colors.black),
          ),
        ],
      ),
    );
  }
}
