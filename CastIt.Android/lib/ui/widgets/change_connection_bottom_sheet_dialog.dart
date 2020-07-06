import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/playlists/playlists_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../bloc/settings/settings_bloc.dart';
import '../../generated/i18n.dart';
import 'modal_sheet_separator.dart';

class ChangeConnectionBottomSheetDialog extends StatefulWidget {
  final String currentUrl;

  const ChangeConnectionBottomSheetDialog({
    Key key,
    @required this.currentUrl,
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
    return Container(
      margin: const EdgeInsets.only(
        left: 10,
        right: 10,
        bottom: 10,
      ),
      padding: const EdgeInsets.only(left: 20, right: 20, top: 20),
      child: BlocBuilder<SettingsBloc, SettingsState>(
        builder: (ctx, state) => Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: <Widget>[
            ModalSheetSeparator(),
            Row(
              children: <Widget>[
                Align(
                  alignment: Alignment.centerLeft,
                  child: Icon(
                    Icons.warning,
                    size: 36,
                  ),
                ),
                Container(
                  margin: const EdgeInsets.only(left: 5),
                  child: Text(
                    i18n.noConnectionToDesktopApp,
                    style: Theme.of(context).textTheme.headline6,
                  ),
                ),
              ],
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
                suffixIcon: IconButton(
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
                  onPressed: () {
                    Navigator.pop(context);
                  },
                  child: Text(
                    i18n.cancel,
                    style: TextStyle(color: theme.primaryColor),
                  ),
                ),
              ],
            )
          ],
        ),
      ),
    );
  }

  void _urlChanged() {
    context.bloc<SettingsBloc>().add(SettingsEvent.castItUrlChanged(castItUrl: _urlController.text));
  }

  void _onRefreshClick() {
    context.bloc<PlayListsBloc>().add(PlayListsEvent.load());
    context.bloc<ServerWsBloc>().add(ServerWsEvent.updateUrlAndConnectToWs(castItUrl: _urlController.text));
    Navigator.of(context).pop();
  }
}
