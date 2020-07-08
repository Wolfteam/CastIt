import 'package:flutter/material.dart';

class Styles {
  //Settings
  static const String appIconPath = 'assets/icon/icon.png';
  static final RoundedRectangleBorder cardSettingsShape =
      RoundedRectangleBorder(borderRadius: BorderRadius.circular(10));

  static const cardSettingsMargin = EdgeInsets.all(10);
  static const double cardSettingsElevation = 3;
  static const cardSettingsContainerMargin = EdgeInsets.all(10);
  static const cardSettingsContainerPadding = EdgeInsets.all(5);

  static const modalBottomSheetShape = RoundedRectangleBorder(
    borderRadius: BorderRadius.only(
      topRight: Radius.circular(35),
      topLeft: Radius.circular(35),
    ),
  );
  static const modalBottomSheetContainerMargin = EdgeInsets.only(left: 10, right: 10, bottom: 10);
  static const modalBottomSheetContainerPadding = EdgeInsets.only(left: 20, right: 20, top: 20);
}
