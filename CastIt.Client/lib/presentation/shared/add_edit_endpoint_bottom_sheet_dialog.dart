import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class AddEditEndpointBottomSheetDialog extends StatefulWidget {
  final Receiver? endpoint;

  const AddEditEndpointBottomSheetDialog({super.key, this.endpoint});

  @override
  State<AddEditEndpointBottomSheetDialog> createState() => _AddEditEndpointBottomSheetDialogState();
}

class _AddEditEndpointBottomSheetDialogState extends State<AddEditEndpointBottomSheetDialog> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _nameController;
  late TextEditingController _hostController;
  late TextEditingController _portController;

  bool get _isEditing => widget.endpoint != null;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.endpoint?.friendlyName ?? '');
    _hostController = TextEditingController(text: widget.endpoint?.host ?? '');
    _portController = TextEditingController(text: widget.endpoint?.port.toString() ?? '8009');
  }

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return CommonBottomSheet(
      title: _isEditing ? i18n.editDevice : i18n.addDevice,
      titleIcon: _isEditing ? Icons.edit : Icons.add,
      okText: i18n.save,
      onOk: _onSave,
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: <Widget>[
            TextFormField(
              controller: _nameController,
              textInputAction: TextInputAction.next,
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) => (value == null || value.trim().isEmpty) ? i18n.invalidName : null,
              decoration: InputDecoration(
                alignLabelWithHint: true,
                hintText: i18n.deviceName,
                labelText: i18n.deviceName,
              ),
            ),
            TextFormField(
              controller: _hostController,
              textInputAction: TextInputAction.next,
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) => (value == null || value.trim().isEmpty) ? i18n.invalidHost : null,
              decoration: InputDecoration(
                alignLabelWithHint: true,
                hintText: i18n.host,
                labelText: i18n.host,
              ),
            ),
            TextFormField(
              controller: _portController,
              keyboardType: TextInputType.number,
              inputFormatters: [FilteringTextInputFormatter.digitsOnly],
              textInputAction: TextInputAction.done,
              autovalidateMode: AutovalidateMode.onUserInteraction,
              validator: (value) {
                final port = int.tryParse(value ?? '');
                return (port == null || port < 1 || port > 65535) ? i18n.invalidPort : null;
              },
              decoration: InputDecoration(
                alignLabelWithHint: true,
                hintText: i18n.port,
                labelText: i18n.port,
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _nameController.dispose();
    _hostController.dispose();
    _portController.dispose();
    super.dispose();
  }

  void _onSave() {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final name = _nameController.text.trim();
    final host = _hostController.text.trim();
    final port = int.parse(_portController.text.trim());
    final bloc = context.read<DevicesBloc>();
    if (_isEditing) {
      bloc.add(DevicesEvent.editEndpoint(id: int.parse(widget.endpoint!.id), name: name, host: host, port: port));
    } else {
      bloc.add(DevicesEvent.addEndpoint(name: name, host: host, port: port));
    }
    Navigator.pop(context);
  }
}
