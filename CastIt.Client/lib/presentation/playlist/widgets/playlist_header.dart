import 'package:castit/application/bloc.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlist/widgets/item_counter.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayListHeader extends StatefulWidget {
  final bool showSearch;
  final int? itemCount;

  const PlayListHeader({
    super.key,
    required this.showSearch,
    this.itemCount,
  });

  @override
  _PlayListHeaderState createState() => _PlayListHeaderState();
}

class _PlayListHeaderState extends State<PlayListHeader> {
  final _searchFocusNode = FocusNode();
  late TextEditingController _searchBoxTextController;

  @override
  void initState() {
    super.initState();

    _searchBoxTextController = TextEditingController(text: '');
    _searchBoxTextController.addListener(_onSearchTextChanged);
  }

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    if (widget.showSearch) {
      WidgetsBinding.instance.addPostFrameCallback((timeStamp) => _searchFocusNode.requestFocus());
    }
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 10),
      child: Column(
        children: <Widget>[
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: <Widget>[
              Align(
                alignment: Alignment.centerLeft,
                child: TextButton.icon(
                  icon: const Icon(Icons.arrow_back),
                  onPressed: () => Navigator.of(context).pop(),
                  label: Text(
                    i18n.playlists,
                    style: const TextStyle(fontSize: 24),
                  ),
                ),
              ),
              if (widget.itemCount != null)
                Container(
                  margin: const EdgeInsets.only(right: 20),
                  child: ItemCounter(widget.itemCount!),
                ),
            ],
          ),
          if (widget.showSearch)
            Card(
              elevation: 10,
              margin: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
              shape: Styles.floatingCardShape,
              child: Row(
                children: <Widget>[
                  Container(
                    margin: const EdgeInsets.only(left: 10),
                    child: const Icon(Icons.search, size: 30),
                  ),
                  Expanded(
                    child: TextField(
                      controller: _searchBoxTextController,
                      focusNode: _searchFocusNode,
                      cursorColor: theme.colorScheme.secondary,
                      keyboardType: TextInputType.text,
                      textInputAction: TextInputAction.go,
                      decoration: InputDecoration(
                        border: InputBorder.none,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 15),
                        hintText: '${i18n.search}...',
                      ),
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: _cleanSearchText,
                    splashRadius: Styles.smallButtonSplashRadius,
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }

  void _onSearchTextChanged() => context.read<PlayListBloc>().add(PlayListEvent.searchBoxTextChanged(text: _searchBoxTextController.text));

  void _cleanSearchText() {
    if (_searchBoxTextController.text.isEmpty) {
      _toggleSearchBoxVisibility();
      return;
    }
    _searchBoxTextController.text = '';
  }

  void _toggleSearchBoxVisibility() => context.read<PlayListBloc>().add(const PlayListEvent.toggleSearchBoxVisibility());
}
