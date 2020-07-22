import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/server_ws/server_ws_bloc.dart';
import '../../../bloc/settings/settings_bloc.dart';
import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'modal_sheet_separator.dart';

class ChangeConnectionBottomSheetDialog extends StatefulWidget {
  final String currentUrl;
  final String title;
  final IconData icon;
  final bool showRefreshButton;
  final bool showOkButton;
  final Function(String) onOk;
  final Function onCancel;

  const ChangeConnectionBottomSheetDialog({
    Key key,
    @required this.currentUrl,
    this.title,
    this.icon,
    this.showOkButton = false,
    this.showRefreshButton = true,
    this.onOk,
    this.onCancel,
  }) : super(key: key);

  @override
  _ChangeConnectionBottomSheetDialogState createState() => _ChangeConnectionBottomSheetDialogState();
}

class _ChangeConnectionBottomSheetDialogState extends State<ChangeConnectionBottomSheetDialog> {
  TextEditingController _urlController;

  @override
  void initState() {
    _urlController = TextEditingController(text: widget.currentUrl);
    _urlController.addListener(_urlChanged);
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = I18n.of(context);
    return SingleChildScrollView(
      padding: EdgeInsets.only(bottom: MediaQuery.of(context).viewInsets.bottom),
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: BlocBuilder<SettingsBloc, SettingsState>(
          builder: (ctx, state) => Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: <Widget>[
              ModalSheetSeparator(),
              BottomSheetTitle(
                title: widget.title ?? i18n.noConnectionToDesktopApp,
                icon: widget.icon ?? Icons.warning,
              ),
              TextFormField(
                maxLines: 1,
                minLines: 1,
                validator: (_) => (state as SettingsLoadedState).isCastItUrlValid ? null : i18n.invalidUrl,
                autovalidate: true,
                controller: _urlController,
                keyboardType: TextInputType.url,
                textInputAction: TextInputAction.done,
                decoration: InputDecoration(
                  suffixIcon: !widget.showRefreshButton
                      ? null
                      : IconButton(
                          alignment: Alignment.bottomCenter,
                          icon: Icon(Icons.sync),
                          onPressed: !(state as SettingsLoadedState).isCastItUrlValid ? null : _onRefreshClick,
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
                    Text(
                      i18n.verifyCastItUrl,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.caption,
                    ),
                    Text(
                      i18n.makeSureYouAreConnected,
                      textAlign: TextAlign.center,
                      style: theme.textTheme.caption,
                    )
                  ],
                ),
              ),
              ButtonBar(
                buttonPadding: const EdgeInsets.symmetric(horizontal: 10),
                children: <Widget>[
                  OutlineButton(
                    onPressed: widget.onCancel != null ? () => widget.onCancel() : _onCancel,
                    child: Text(
                      i18n.cancel,
                      style: TextStyle(color: theme.primaryColor),
                    ),
                  ),
                  if (widget.showOkButton)
                    RaisedButton(
                      color: theme.primaryColor,
                      onPressed: !(state as SettingsLoadedState).isCastItUrlValid
                          ? null
                          : widget.onOk != null ? () => widget.onOk(_urlController.text) : _onRefreshClick,
                      child: Text(i18n.ok),
                    )
                ],
              )
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _urlController.dispose();
    super.dispose();
  }

  void _urlChanged() {
    context.bloc<SettingsBloc>().add(SettingsEvent.castItUrlChanged(castItUrl: _urlController.text));
  }

  void _onCancel() => Navigator.pop(context);

  void _onRefreshClick() {
    // context.bloc<PlayListsBloc>().add(PlayListsEvent.load());
    context.bloc<ServerWsBloc>().add(ServerWsEvent.updateUrlAndConnectToWs(castItUrl: _urlController.text));
    _onCancel();
  }
}
