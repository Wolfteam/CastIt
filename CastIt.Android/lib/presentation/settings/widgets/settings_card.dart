import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';

class SettingsCard extends StatelessWidget {
  final Widget child;

  const SettingsCard({required this.child});

  @override
  Widget build(BuildContext context) {
    return Card(
      shape: Styles.cardSettingsShape,
      margin: Styles.cardSettingsMargin,
      elevation: Styles.cardSettingsElevation,
      child: Container(
        margin: Styles.cardSettingsContainerMargin,
        padding: Styles.cardSettingsContainerPadding,
        child: child,
      ),
    );
  }
}
