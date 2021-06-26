import 'package:flutter/material.dart';

import '../../generated/i18n.dart';

class SomethingWentWrong extends StatelessWidget {
  const SomethingWentWrong();
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = I18n.of(context)!;
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 10),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: <Widget>[
          const Icon(Icons.info_outline, size: 60),
          Text(i18n.somethingWentWrong, textAlign: TextAlign.center, style: theme.textTheme.headline4),
          Text(i18n.pleaseTryAgainLater, textAlign: TextAlign.center, style: theme.textTheme.headline5),
          Text(i18n.makeSureYouAreConnected, textAlign: TextAlign.center, style: theme.textTheme.subtitle1),
        ],
      ),
    );
  }
}
