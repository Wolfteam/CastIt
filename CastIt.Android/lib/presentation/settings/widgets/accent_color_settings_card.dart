import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/common_dropdown_button.dart';
import 'package:castit/presentation/shared/extensions/app_theme_type_extensions.dart';
import 'package:castit/presentation/shared/utils/enum_utils.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AccentColorSettingsCard extends StatelessWidget {
  const AccentColorSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Row(
            children: <Widget>[
              const Icon(Icons.colorize),
              Container(
                margin: const EdgeInsets.only(left: 5),
                child: Text(
                  i18n.accentColor,
                  style: Theme.of(context).textTheme.headline6,
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(
              i18n.chooseAccentColor,
              style: const TextStyle(color: Colors.grey),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => CommonDropdownButton<AppAccentColorType>(
                hint: i18n.chooseAccentColor,
                showSubTitle: false,
                values: AppAccentColorType.values.map((e) => TranslatedEnum(e, _formatEnumName(e))).toList()
                  ..sort((x, y) => x.translation.compareTo(y.translation)),
                leadingIconBuilder: (val) => Container(
                  margin: const EdgeInsets.only(right: 10),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(10),
                    color: val.getAccentColor(),
                  ),
                  width: 20,
                  height: 20,
                ),
                currentValue: state.accentColor,
                onChanged: (newValue, _) => _accentColorChanged(context, newValue),
              ),
              orElse: () => const CircularProgressIndicator(),
            ),
          ),
        ],
      ),
    );
  }

  String _formatEnumName(AppAccentColorType color) {
    final name = color.name;
    final words = name.split(RegExp('(?<=[a-z])(?=[A-Z])'));
    return words.map((e) => e.toCapitalized()).join(' ');
  }

  void _accentColorChanged(BuildContext context, AppAccentColorType newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.accentColorChanged(accentColor: newValue));
    context.read<MainBloc>().add(MainEvent.accentColorChanged(accentColor: newValue));
  }
}
