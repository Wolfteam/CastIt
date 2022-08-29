import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/about_settings_card.dart';
import 'package:castit/presentation/settings/widgets/accent_color_settings_card.dart';
import 'package:castit/presentation/settings/widgets/language_settings_card.dart';
import 'package:castit/presentation/settings/widgets/player_settings_card.dart';
import 'package:castit/presentation/settings/widgets/theme_settings_card.dart';
import 'package:castit/presentation/shared/page_header.dart';
import 'package:flutter/material.dart';

class SettingsPage extends StatefulWidget {
  @override
  _SettingsPageState createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> with AutomaticKeepAliveClientMixin<SettingsPage> {
  final _scrollController = ScrollController();

  @override
  bool get wantKeepAlive => true;

  @override
  Widget build(BuildContext context) {
    super.build(context);
    final i18n = S.of(context);
    return ListView.builder(
      shrinkWrap: true,
      itemCount: 6,
      controller: _scrollController,
      itemBuilder: (ctx, index) {
        switch (index) {
          case 0:
            return PageHeader(
              title: i18n.settings,
              icon: Icons.settings,
            );
          case 1:
            return const ThemeSettingsCard();
          case 2:
            return const AccentColorSettingsCard();
          case 3:
            return const LanguageSettingsCard();
          case 4:
            return const PlayerSettingsCard();
          case 5:
            return const AboutSettingsCard();
          default:
            throw Exception('Invalid index');
        }
      },
    );
  }
}
