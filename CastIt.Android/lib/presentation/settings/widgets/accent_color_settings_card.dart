import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/extensions/app_theme_type_extensions.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AccentColorSettingsCard extends StatelessWidget {
  const AccentColorSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
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
              loaded: (state) => GridView.count(
                shrinkWrap: true,
                primary: false,
                padding: const EdgeInsets.all(20),
                crossAxisSpacing: 10,
                mainAxisSpacing: 10,
                crossAxisCount: 5,
                children: AppAccentColorType.values.map((val) => _AccentColorCell(value: val, isSelected: state.accentColor == val)).toList(),
              ),
              orElse: () => const CircularProgressIndicator(),
            ),
          ),
        ],
      ),
    );
  }
}

class _AccentColorCell extends StatelessWidget {
  final AppAccentColorType value;
  final bool isSelected;

  const _AccentColorCell({required this.value, required this.isSelected});

  @override
  Widget build(BuildContext context) {
    final color = value.getAccentColor();
    return InkWell(
      onTap: () => _accentColorChanged(context, value),
      child: Container(
        padding: const EdgeInsets.all(8),
        color: color,
        child: isSelected ? const Icon(Icons.check, color: Colors.white) : null,
      ),
    );
  }

  void _accentColorChanged(BuildContext context, AppAccentColorType newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.accentColorChanged(accentColor: newValue));
    context.read<MainBloc>().add(MainEvent.accentColorChanged(accentColor: newValue));
  }
}
