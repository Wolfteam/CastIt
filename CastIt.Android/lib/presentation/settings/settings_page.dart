import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/about_settings_card.dart';
import 'package:castit/presentation/settings/widgets/accent_color_settings_card.dart';
import 'package:castit/presentation/settings/widgets/language_settings_card.dart';
import 'package:castit/presentation/settings/widgets/player_settings_card.dart';
import 'package:castit/presentation/settings/widgets/theme_settings_card.dart';
import 'package:castit/presentation/shared/page_header.dart';
import 'package:flutter/material.dart';
import 'package:responsive_builder/responsive_builder.dart';

class SettingsPage extends StatelessWidget {
  const SettingsPage({super.key});

  @override
  Widget build(BuildContext context) {
    final isPortrait = MediaQuery.of(context).orientation == Orientation.portrait;
    return Scaffold(
      body: SafeArea(
        child: ResponsiveBuilder(
          builder: (ctx, size) => isPortrait ? const _MobileLayout() : const _DesktopTabletLayout(),
        ),
      ),
    );
  }
}

class _MobileLayout extends StatefulWidget {
  const _MobileLayout();

  @override
  State<_MobileLayout> createState() => _MobileLayoutState();
}

class _MobileLayoutState extends State<_MobileLayout> with AutomaticKeepAliveClientMixin<_MobileLayout> {
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

class _DesktopTabletLayout extends StatelessWidget {
  const _DesktopTabletLayout();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return ListView(
      padding: const EdgeInsets.all(10),
      shrinkWrap: true,
      children: [
        PageHeader(
          title: i18n.settings,
          icon: Icons.settings,
        ),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Column(
                children: const [
                  ThemeSettingsCard(),
                  AccentColorSettingsCard(),
                  LanguageSettingsCard(),
                  AboutSettingsCard(),
                ],
              ),
            ),
            Expanded(
              child: Column(
                children: const [
                  PlayerSettingsCard(),
                ],
              ),
            ),
          ],
        ),
      ],
    );
  }
}
