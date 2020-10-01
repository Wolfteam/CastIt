import 'package:flutter/material.dart';

import '../../../common/styles.dart';

class IntroPageItem extends StatelessWidget {
  final String mainTitle;
  final String subTitle;
  final String content;
  final Widget extraContent;

  const IntroPageItem({Key key, this.mainTitle, this.subTitle, this.content, this.extraContent}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      color: theme.backgroundColor.withOpacity(0.3),
      padding: const EdgeInsets.symmetric(horizontal: 20),
      alignment: Alignment.center,
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: <Widget>[
          Image.asset(Styles.appIconPath, width: 200, height: 200),
          Text(
            mainTitle,
            textAlign: TextAlign.center,
            style: const TextStyle(fontWeight: FontWeight.w500, fontSize: 24),
          ),
          const SizedBox(height: 10),
          Text(subTitle),
          const SizedBox(height: 20),
          Text(
            content,
            textAlign: TextAlign.center,
            style: const TextStyle(fontWeight: FontWeight.w500, fontSize: 20),
          ),
          const SizedBox(height: 20),
          if (extraContent != null) extraContent
        ],
      ),
    );
  }
}
