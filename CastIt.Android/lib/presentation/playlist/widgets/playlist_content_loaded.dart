import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/playlist/widgets/file_item.dart';
import 'package:castit/presentation/playlist/widgets/playlist_header.dart';
import 'package:castit/presentation/shared/nothing_found_column.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:pull_to_refresh/pull_to_refresh.dart';

class PlayListContentLoaded extends StatelessWidget {
  final int playListId;
  final bool isLoaded;
  final List<FileItemResponseDto> files;
  final bool searchBoxIsVisible;
  final double itemHeight;
  final RefreshController refreshController;
  final ScrollController listViewScrollController;

  const PlayListContentLoaded({
    Key? key,
    required this.playListId,
    required this.isLoaded,
    required this.files,
    required this.searchBoxIsVisible,
    required this.itemHeight,
    required this.refreshController,
    required this.listViewScrollController,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);

    if (!isLoaded) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          const PlayListHeader(showSearch: false),
          Expanded(
            child: Center(
              child: Text(s.somethingWentWrong),
            ),
          ),
        ],
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        PlayListHeader(itemCount: files.length, showSearch: searchBoxIsVisible),
        Expanded(
          child: SmartRefresher(
            header: const MaterialClassicHeader(),
            controller: refreshController,
            onRefresh: () => context.read<PlayListBloc>().add(PlayListEvent.load(id: playListId)),
            child: files.isEmpty
                ? const NothingFoundColumn()
                : ListView.builder(
                    controller: listViewScrollController,
                    shrinkWrap: true,
                    itemCount: files.length,
                    itemBuilder: (ctx, i) => FileItem.fromItem(key: Key('file_item_$i'), itemHeight: itemHeight, file: files[i]),
                  ),
          ),
        ),
      ],
    );
  }
}
