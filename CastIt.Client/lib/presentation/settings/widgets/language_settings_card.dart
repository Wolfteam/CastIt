import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/common_dropdown_button.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/utils/enum_utils.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class LanguageSettingsCard extends StatelessWidget {
  const LanguageSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
            children: <Widget>[
              const Icon(Icons.language),
              Container(
                margin: const EdgeInsets.only(left: 5),
                child: Text(
                  i18n.language,
                  style: Theme.of(context).textTheme.titleLarge,
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(
              i18n.chooseLanguage,
              style: const TextStyle(color: Colors.grey),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => CommonDropdownButton<AppLanguageType>(
                hint: i18n.chooseLanguage,
                showSubTitle: false,
                values: [AppLanguageType.english, AppLanguageType.spanish].map((e) => TranslatedEnum(e, i18n.translateAppLanguageType(e))).toList(),
                currentValue: state.appLanguage,
                onChanged: (newValue, _) => context.read<SettingsBloc>().add(SettingsEvent.languageChanged(lang: newValue)),
              ),
              orElse: () => const CircularProgressIndicator(),
            ),
          ),
        ],
      ),
    );
  }
}
