import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../bloc/server_ws/server_ws_bloc.dart';
import '../../../common/extensions/string_extensions.dart';
import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import '../../pages/playlist_page.dart';
import '../modals/played_file_options_bottom_sheet_dialog.dart';

class PlayCoverImg extends StatelessWidget {
  final int? fileId;
  final String? thumbUrl;
  final int? playListId;
  final String? playListName;
  final String? fileName;
  final bool showLoading;
  final bool loopFile;
  final bool loopPlayList;
  final bool shufflePlayList;

  bool get fileIdIsValid => fileId != null && fileId! > 0;

  bool get playListIsValid => playListId != null && playListId! > 0;

  const PlayCoverImg({
    Key? key,
    this.fileId,
    this.playListId,
    this.playListName,
    this.fileName,
    this.thumbUrl,
    this.loopFile = false,
    this.loopPlayList = false,
    this.shufflePlayList = false,
    this.showLoading = false,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    const dummyIndicator = Center(child: CircularProgressIndicator());
    return Stack(
      children: <Widget>[
        if (!thumbUrl.isNullEmptyOrWhitespace && !showLoading)
          CachedNetworkImage(
            imageUrl: thumbUrl!,
            imageBuilder: (ctx, imageProvider) => Container(
              decoration: BoxDecoration(
                image: DecorationImage(
                  image: imageProvider,
                  fit: BoxFit.fill,
                ),
              ),
            ),
            placeholder: (ctx, url) => dummyIndicator,
            errorWidget: (ctx, url, error) => Container(),
          ),
        if (showLoading) dummyIndicator,
        Container(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [Colors.black.withOpacity(0.1), Colors.black.withOpacity(0.5)],
              begin: Alignment.center,
              end: Alignment.topCenter,
            ),
          ),
        ),
        Container(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [Colors.black.withOpacity(0.1), Colors.black.withOpacity(0.8)],
              begin: Alignment.center,
              end: Alignment.bottomCenter,
            ),
          ),
        ),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 10.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              const SizedBox(height: 20.0),
              _buildTop(context),
              const Spacer(),
              _buildBottom(context),
            ],
          ),
        )
      ],
    );
  }

  Widget _buildTop(BuildContext context) {
    final i18n = I18n.of(context)!;
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: <Widget>[
        IconButton(
          icon: const Icon(
            Icons.playlist_play,
            color: Colors.white,
          ),
          onPressed: !fileIdIsValid && !playListIsValid ? null : () => _goToPlayList(context),
        ),
        Expanded(
          child: Column(
            children: <Widget>[
              Text(
                i18n.playlist,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(color: Colors.white.withOpacity(0.6)),
              ),
              Text(
                playListName.isNullEmptyOrWhitespace ? '' : playListName!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.white),
              ),
            ],
          ),
        ),
        IconButton(
          icon: const Icon(
            Icons.settings,
            color: Colors.white,
          ),
          onPressed: !fileIdIsValid ? null : () => _showFileOptionsModal(context),
        )
      ],
    );
  }

  Widget _buildBottom(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = I18n.of(context)!;
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 16),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: <Widget>[
          IconButton(
            tooltip: i18n.shufflePlayList,
            icon: Icon(Icons.shuffle, color: shufflePlayList ? theme.accentColor : Colors.white),
            onPressed: !playListIsValid ? null : () => _togglePlayListShuffle(context),
          ),
          Flexible(
            child: Tooltip(
              message: fileName.isNullEmptyOrWhitespace ? i18n.na : fileName!,
              child: Text(
                fileName.isNullEmptyOrWhitespace ? '' : fileName!,
                overflow: TextOverflow.ellipsis,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 28.0),
              ),
            ),
          ),
          IconButton(
            tooltip: i18n.loopFile,
            icon: Icon(Icons.repeat, color: loopFile ? theme.accentColor : Colors.white),
            onPressed: !fileIdIsValid ? null : () => _toggleFileLoop(context),
          ),
        ],
      ),
    );
  }

  Future<void> _goToPlayList(BuildContext context) async {
    await PlayListPage.forDetails(playListId!, fileId, context);
  }

  void _showFileOptionsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => PlayedFileOptionsBottomSheetDialog(),
    );
  }

  Future<void> _togglePlayListShuffle(BuildContext context) =>
      context.read<ServerWsBloc>().setPlayListOptions(playListId!, loop: loopPlayList, shuffle: !shufflePlayList);

  Future<void> _toggleFileLoop(BuildContext context) => context.read<ServerWsBloc>().loopFile(fileId!, playListId!, loop: !loopFile);
}
