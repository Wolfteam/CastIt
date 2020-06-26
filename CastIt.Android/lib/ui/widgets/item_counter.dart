import 'package:flutter/material.dart';

class ItemCounter extends StatelessWidget {
  final int _items;
  const ItemCounter(
    this._items,
  );

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        color: Colors.pink.withOpacity(0.5),
        borderRadius: BorderRadius.circular(20.0),
      ),
      child: Align(
        alignment: Alignment.center,
        child: Text(
          '$_items',
          textAlign: TextAlign.center,
          style: TextStyle(
            fontSize: 20,
          ),
        ),
      ),
    );
  }
}
