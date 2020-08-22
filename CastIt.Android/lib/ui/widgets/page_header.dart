import 'package:flutter/material.dart';

class PageHeader extends StatelessWidget {
  final String title;
  final IconData icon;
  final double iconSize;
  final EdgeInsetsGeometry margin;
  const PageHeader({
    Key key,
    @required this.title,
    @required this.icon,
    this.iconSize = 40,
    this.margin = const EdgeInsets.symmetric(vertical: 20, horizontal: 10),
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: <Widget>[
        Container(
          margin: margin,
          child: Icon(
            icon,
            size: iconSize,
          ),
        ),
        Flexible(
          child: Container(
            padding: const EdgeInsets.only(left: 5),
            child: Text(
              title,
              textAlign: TextAlign.start,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                fontSize: 28,
              ),
            ),
          ),
        ),
      ],
    );
  }
}
