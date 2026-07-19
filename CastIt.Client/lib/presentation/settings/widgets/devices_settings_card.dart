import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/devices_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DevicesSettingsCard extends StatelessWidget {
  const DevicesSettingsCard();

  @override
  Widget build(BuildContext context) {
    //The devices card is only useful while connected to the server
    return BlocBuilder<SettingsBloc, SettingsState>(
      builder: (context, state) => switch (state) {
        SettingsStateLoadingState() => const SizedBox.shrink(),
        SettingsStateLoadedState() => !state.isConnected ? const SizedBox.shrink() : _buildCard(context),
      },
    );
  }

  Widget _buildCard(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: <Widget>[
              const Icon(Icons.cast),
              Container(margin: const EdgeInsets.only(left: 5), child: Text(i18n.devices, style: textTheme.titleLarge)),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(i18n.manageDevices, style: const TextStyle(color: Colors.grey)),
          ),
          InkWell(
            onTap: () => _showDevicesModal(context),
            child: Container(
              constraints: const BoxConstraints(minHeight: kToolbarHeight),
              margin: const EdgeInsets.only(top: 10),
              child: Row(
                children: [
                  const Padding(
                    padding: EdgeInsets.only(left: 16, right: 16),
                    child: Icon(Icons.cast_connected),
                  ),
                  Expanded(child: Text(i18n.devices, style: textTheme.titleMedium)),
                  const Icon(Icons.chevron_right),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  void _showDevicesModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => const DevicesBottomSheetDialog(),
    );
  }
}
