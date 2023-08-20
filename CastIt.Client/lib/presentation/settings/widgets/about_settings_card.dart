import 'package:castit/application/bloc.dart';
import 'package:castit/domain/app_constants.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/settings/widgets/settings_link.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AboutSettingsCard extends StatelessWidget {
  const AboutSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final textTheme = Theme.of(context).textTheme;
    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
            children: <Widget>[
              const Icon(Icons.info_outline),
              Container(
                margin: const EdgeInsets.only(left: 5),
                child: Text(
                  i18n.about,
                  style: textTheme.titleLarge,
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(
              i18n.appInfo,
              style: const TextStyle(color: Colors.grey),
            ),
          ),
          Container(
            margin: const EdgeInsets.only(left: 16, right: 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                Image.asset(
                  Styles.appIconPath,
                  width: 70,
                  height: 70,
                  fit: BoxFit.contain,
                ),
                Text(
                  i18n.appName,
                  textAlign: TextAlign.center,
                  style: textTheme.titleSmall,
                ),
                BlocBuilder<SettingsBloc, SettingsState>(
                  builder: (context, state) => state.maybeMap(
                    loaded: (state) => Text(
                      i18n.appVersion(state.appVersion),
                      textAlign: TextAlign.center,
                      style: textTheme.titleSmall,
                    ),
                    orElse: () => const CircularProgressIndicator(),
                  ),
                ),
                Text(
                  i18n.aboutSummary,
                  textAlign: TextAlign.center,
                ),
                SettingsLink(title: i18n.desktopApp, url: AppConstants.githubReleasePage),
                Container(
                  margin: const EdgeInsets.only(top: 10),
                  child: Text(
                    i18n.donations,
                    style: textTheme.titleMedium!.copyWith(fontWeight: FontWeight.bold),
                  ),
                ),
                Text(i18n.donationsMsg),
                Container(
                  margin: const EdgeInsets.only(top: 10),
                  child: Text(
                    i18n.support,
                    style: textTheme.titleMedium!.copyWith(fontWeight: FontWeight.bold),
                  ),
                ),
                Container(
                  margin: const EdgeInsets.only(top: 5),
                  child: Text(i18n.donationSupport),
                ),
                SettingsLink(title: i18n.issues, url: AppConstants.githubIssuesPage),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
