import 'package:flutter/material.dart';

class BottomSheetTitle extends StatelessWidget {
  final String title;
  final IconData icon;
  const BottomSheetTitle({
    super.key,
    required this.title,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Row(
      children: <Widget>[
        Align(alignment: Alignment.centerLeft, child: Icon(icon, size: 36)),
        Expanded(
          child: Container(
            margin: const EdgeInsets.only(left: 5),
            child: Text(title, style: theme.textTheme.titleLarge),
          ),
        ),
      ],
    );
  }
}
