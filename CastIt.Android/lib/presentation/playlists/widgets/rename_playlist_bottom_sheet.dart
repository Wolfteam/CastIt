import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class RenamePlayListBottomSheet extends StatefulWidget {
  final int id;
  final String currentName;

  const RenamePlayListBottomSheet({
    Key? key,
    required this.id,
    required this.currentName,
  }) : super(key: key);

  @override
  _RenamePlayListBottomSheetState createState() => _RenamePlayListBottomSheetState();
}

class _RenamePlayListBottomSheetState extends State<RenamePlayListBottomSheet> {
  TextEditingController? _nameController;
  bool _didChangeDependencies = false;

  @override
  void initState() {
    super.initState();
    _nameController = TextEditingController(text: widget.currentName);
    _nameController!.addListener(_nameChanged);
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();

    if (_didChangeDependencies) {
      return;
    }
    context.read<PlayListRenameBloc>().add(PlayListRenameEvent.load(name: widget.currentName));
    _didChangeDependencies = true;
  }

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return BlocBuilder<PlayListRenameBloc, PlayListRenameState>(
      builder: (ctx, state) => CommonBottomSheet(
        title: i18n.rename,
        titleIcon: Icons.edit,
        okText: i18n.rename,
        onOk: !state.isNameValid ? null : _rename,
        onCancel: _cancel,
        child: TextFormField(
          autofocus: true,
          minLines: 1,
          validator: (_) => state.isNameValid ? null : i18n.invalidName,
          autovalidateMode: AutovalidateMode.always,
          controller: _nameController,
          keyboardType: TextInputType.text,
          textInputAction: TextInputAction.done,
          decoration: InputDecoration(
            suffixIcon: IconButton(
              alignment: Alignment.bottomCenter,
              icon: const Icon(Icons.close),
              onPressed: () => _nameController!.clear(),
            ),
            alignLabelWithHint: true,
            hintText: i18n.name,
            labelText: i18n.name,
          ),
        ),
      ),
    );
  }

  void _nameChanged() => context.read<PlayListRenameBloc>().add(PlayListRenameEvent.nameChanged(name: _nameController!.text));

  void _cancel() => Navigator.of(context).pop();

  void _rename() {
    context.read<ServerWsBloc>().updatePlayList(widget.id, _nameController!.text);
    _cancel();
  }
}
