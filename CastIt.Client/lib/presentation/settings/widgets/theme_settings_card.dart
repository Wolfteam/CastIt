import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/common_dropdown_button.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/utils/enum_utils.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class ThemeSettingsCard extends StatelessWidget {
  const ThemeSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
            children: <Widget>[
              const Icon(Icons.color_lens),
              Container(
                margin: const EdgeInsets.only(left: 5),
                child: Text(
                  i18n.theme,
                  style: Theme.of(context).textTheme.headline6,
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(
              i18n.chooseBaseAppColor,
              style: const TextStyle(color: Colors.grey),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => CommonDropdownButton<AppThemeType>(
                hint: i18n.chooseBaseAppColor,
                showSubTitle: false,
                values: AppThemeType.values.map((e) => TranslatedEnum(e, i18n.translateAppThemeType(e))).toList(),
                currentValue: state.appTheme,
                onChanged: (newValue, _) => _appThemeChanged(context, newValue),
              ),
              orElse: () => const CircularProgressIndicator(),
            ),
          ),
        ],
      ),
    );
  }

  void _appThemeChanged(BuildContext context, AppThemeType? newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.themeChanged(theme: newValue!));
    context.read<MainBloc>().add(MainEvent.themeChanged(theme: newValue));
  }
}
