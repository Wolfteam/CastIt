import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/add_edit_endpoint_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class DevicesBottomSheetDialog extends StatelessWidget {
  const DevicesBottomSheetDialog({super.key});

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return CommonBottomSheet(
      title: i18n.devices,
      titleIcon: Icons.cast,
      showOkButton: false,
      child: BlocBuilder<DevicesBloc, DevicesState>(
        builder: (ctx, state) {
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: <Widget>[
                  IconButton(
                    tooltip: i18n.addDevice,
                    icon: const Icon(Icons.add),
                    onPressed: () => _showAddEditForm(context),
                  ),
                  IconButton(
                    tooltip: i18n.refreshDevices,
                    icon: const Icon(Icons.refresh),
                    onPressed: () => context.read<DevicesBloc>().add(const DevicesEvent.refresh()),
                  ),
                ],
              ),
              if (state.devices.isEmpty)
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 20),
                  child: Text(i18n.noDevicesFound, textAlign: TextAlign.center),
                )
              else
                ...state.devices.map((device) => _DeviceItem(device: device)),
            ],
          );
        },
      ),
    );
  }

  static void _showAddEditForm(BuildContext context, {Receiver? endpoint}) {
    final bloc = context.read<DevicesBloc>();
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => BlocProvider.value(
        value: bloc,
        child: AddEditEndpointBottomSheetDialog(endpoint: endpoint),
      ),
    );
  }
}

class _DeviceItem extends StatelessWidget {
  final Receiver device;

  const _DeviceItem({required this.device});

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return ListTile(
      contentPadding: EdgeInsets.zero,
      leading: Icon(device.isConnected ? Icons.cast_connected : Icons.tv),
      title: Text(device.friendlyName, overflow: TextOverflow.ellipsis),
      subtitle: Text('${device.host}:${device.port}', overflow: TextOverflow.ellipsis),
      trailing: Row(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          if (device.isCustom) ...[
            IconButton(
              tooltip: i18n.editDevice,
              icon: const Icon(Icons.edit),
              onPressed: () => DevicesBottomSheetDialog._showAddEditForm(context, endpoint: device),
            ),
            IconButton(
              tooltip: i18n.delete,
              icon: const Icon(Icons.delete),
              onPressed: () =>
                  context.read<DevicesBloc>().add(DevicesEvent.deleteEndpoint(id: int.parse(device.id))),
            ),
          ],
          IconButton(
            tooltip: device.isConnected ? i18n.disconnect : i18n.connect,
            icon: Icon(device.isConnected ? Icons.cast_connected : Icons.cast),
            onPressed: () => context
                .read<DevicesBloc>()
                .add(DevicesEvent.connect(id: device.isConnected ? null : device.id)),
          ),
        ],
      ),
    );
  }
}
