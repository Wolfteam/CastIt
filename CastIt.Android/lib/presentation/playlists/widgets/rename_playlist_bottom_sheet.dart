import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/bottom_sheet_title.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:castit/presentation/shared/modal_sheet_separator.dart';
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
    final separator = ModalSheetSeparator();
    final sheetTitle = BottomSheetTitle(icon: Icons.edit, title: i18n.rename);
    return SingleChildScrollView(
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: BlocBuilder<PlayListRenameBloc, PlayListRenameState>(
          builder: (ctx, state) => Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: <Widget>[
              separator,
              sheetTitle,
              ..._buildPage(ctx, state),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildPage(BuildContext context, PlayListRenameState state) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    return state.map(
      initial: (s) => [],
      loaded: (s) {
        return [
          TextFormField(
            autofocus: true,
            minLines: 1,
            validator: (_) => s.isNameValid ? null : i18n.invalidName,
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
          ButtonBar(
            buttonPadding: const EdgeInsets.symmetric(horizontal: 10),
            children: <Widget>[
              OutlinedButton(
                onPressed: _cancel,
                child: Text(i18n.cancel, style: TextStyle(color: theme.primaryColor)),
              ),
              ElevatedButton(
                onPressed: !s.isNameValid ? null : _rename,
                child: Text(i18n.rename),
              )
            ],
          )
        ];
      },
    );
  }

  void _nameChanged() => context.read<PlayListRenameBloc>().add(PlayListRenameEvent.nameChanged(name: _nameController!.text));

  void _cancel() => Navigator.of(context).pop();

  void _rename() {
    context.read<ServerWsBloc>().updatePlayList(widget.id, _nameController!.text);
    _cancel();
  }
}
