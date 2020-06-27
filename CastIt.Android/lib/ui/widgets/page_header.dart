import 'package:flutter/material.dart';

class PageHeader extends StatelessWidget {
  final String title;
  final IconData icon;
  final double iconSize;

  const PageHeader({
    Key key,
    @required this.title,
    @required this.icon,
    this.iconSize = 40,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 20, horizontal: 10),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.start,
        children: <Widget>[
          Icon(
            icon,
            size: iconSize,
          ),
          Container(
            margin: const EdgeInsets.only(left: 5),
            child: Text(
              title,
              textAlign: TextAlign.start,
              style: const TextStyle(
                fontSize: 28,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
