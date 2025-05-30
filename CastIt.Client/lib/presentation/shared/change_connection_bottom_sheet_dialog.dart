import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/common_bottom_sheet.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class ChangeConnectionBottomSheetDialog extends StatefulWidget {
  final String currentUrl;
  final String? title;
  final IconData? icon;
  final bool showRefreshButton;
  final bool showOkButton;
  final Function(String)? onOk;
  final VoidCallback? onCancel;

  const ChangeConnectionBottomSheetDialog({
    super.key,
    required this.currentUrl,
    this.title,
    this.icon,
    this.showOkButton = false,
    this.showRefreshButton = true,
    this.onOk,
    this.onCancel,
  });

  @override
  _ChangeConnectionBottomSheetDialogState createState() => _ChangeConnectionBottomSheetDialogState();
}

class _ChangeConnectionBottomSheetDialogState extends State<ChangeConnectionBottomSheetDialog> {
  late TextEditingController _urlController;

  @override
  void initState() {
    _urlController = TextEditingController(text: widget.currentUrl);
    _urlController.addListener(_urlChanged);
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = S.of(context);
    return BlocBuilder<SettingsBloc, SettingsState>(
      builder:
          (ctx, state) => CommonBottomSheet(
            title: widget.title ?? i18n.noConnectionToDesktopApp,
            titleIcon: widget.icon ?? Icons.warning,
            onCancel: widget.onCancel != null ? () => widget.onCancel!() : _onCancel,
            showOkButton: widget.showOkButton,
            onOk: () {
              switch (state) {
                case final SettingsStateLoadedState state when state.isCastItUrlValid:
                  widget.onOk != null ? widget.onOk!(_urlController.text) : _onRefreshClick();
                default:
                  break;
              }
            },
            child: switch (state) {
              SettingsStateLoadingState() => const Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [CircularProgressIndicator()],
              ),
              SettingsStateLoadedState() => Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: <Widget>[
                  TextFormField(
                    minLines: 1,
                    validator: (_) => state.isCastItUrlValid ? null : i18n.invalidUrl,
                    autovalidateMode: AutovalidateMode.always,
                    controller: _urlController,
                    keyboardType: TextInputType.url,
                    textInputAction: TextInputAction.done,
                    decoration: InputDecoration(
                      suffixIcon:
                          !widget.showRefreshButton
                              ? null
                              : IconButton(
                                alignment: Alignment.bottomCenter,
                                icon: const Icon(Icons.sync),
                                onPressed: !state.isCastItUrlValid ? null : _onRefreshClick,
                              ),
                      alignLabelWithHint: true,
                      hintText: i18n.url,
                      labelText: i18n.url,
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.only(top: 15, left: 20, right: 20),
                    child: Column(
                      children: <Widget>[
                        Text(i18n.verifyCastItUrl, textAlign: TextAlign.center, style: theme.textTheme.bodySmall),
                        Text(i18n.makeSureYouAreConnected, textAlign: TextAlign.center, style: theme.textTheme.bodySmall),
                      ],
                    ),
                  ),
                ],
              ),
            },
          ),
    );
  }

  @override
  void dispose() {
    _urlController.dispose();
    super.dispose();
  }

  void _urlChanged() {
    context.read<SettingsBloc>().add(SettingsEvent.castItUrlChanged(castItUrl: _urlController.text));
  }

  void _onCancel() => Navigator.pop(context);

  void _onRefreshClick() {
    // context.read<PlayListsBloc>().add(PlayListsEvent.load());
    context.read<ServerWsBloc>().add(ServerWsEvent.updateUrlAndConnectToWs(castItUrl: _urlController.text));
    _onCancel();
  }
}
