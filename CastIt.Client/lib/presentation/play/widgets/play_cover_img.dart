import 'package:cached_network_image/cached_network_image.dart';
import 'package:castit/application/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/play/widgets/played_file_options_bottom_sheet_dialog.dart';
import 'package:castit/presentation/playlist/playlist_page.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
    super.key,
    this.fileId,
    this.playListId,
    this.playListName,
    this.fileName,
    this.thumbUrl,
    this.loopFile = false,
    this.loopPlayList = false,
    this.shufflePlayList = false,
    this.showLoading = false,
  });

  @override
  Widget build(BuildContext context) {
    const dummyIndicator = Center(child: CircularProgressIndicator());
    final s = S.of(context);
    return Stack(
      children: <Widget>[
        if (!thumbUrl.isNullEmptyOrWhitespace && !showLoading)
          Center(
            child: CachedNetworkImage(
              imageUrl: thumbUrl!,
              fit: BoxFit.fitHeight,
              height: double.maxFinite,
              filterQuality: FilterQuality.high,
              placeholder: (ctx, url) => dummyIndicator,
              errorWidget: (ctx, url, error) => Center(child: Text(s.unknownErrorLoadingFile)),
            ),
          ),
        if (showLoading) dummyIndicator,
        DecoratedBox(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [Colors.black.withValues(alpha: 0.1), Colors.black.withValues(alpha: 0.5)],
              begin: Alignment.center,
              end: Alignment.topCenter,
            ),
          ),
          child: const SizedBox.expand(),
        ),
        DecoratedBox(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [Colors.black.withValues(alpha: 0.1), Colors.black.withValues(alpha: 0.8)],
              begin: Alignment.center,
              end: Alignment.bottomCenter,
            ),
          ),
          child: const SizedBox.expand(),
        ),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 10.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              const SizedBox(height: 20.0),
              _Top(
                fileIdIsValid: fileIdIsValid,
                playListIsValid: playListIsValid,
                fileId: fileId,
                playListId: playListId,
                playListName: playListName,
              ),
              const Spacer(),
              _Bottom(
                fileIdIsValid: fileIdIsValid,
                playListIsValid: playListIsValid,
                fileId: fileId,
                playListId: playListId,
                fileName: fileName,
                loopFile: loopFile,
                loopPlayList: loopPlayList,
                shufflePlayList: shufflePlayList,
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _Top extends StatelessWidget {
  final int? fileId;
  final int? playListId;
  final String? playListName;

  final bool fileIdIsValid;
  final bool playListIsValid;

  const _Top({this.fileId, this.playListId, this.playListName, required this.fileIdIsValid, required this.playListIsValid});

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: <Widget>[
        IconButton(
          icon: const Icon(Icons.playlist_play, color: Colors.white),
          onPressed: !fileIdIsValid && !playListIsValid ? null : () => _goToPlayList(context),
        ),
        Expanded(
          child: Column(
            children: <Widget>[
              Text(i18n.playlist, overflow: TextOverflow.ellipsis, style: TextStyle(color: Colors.white.withValues(alpha: 0.6))),
              Text(
                playListName.isNullEmptyOrWhitespace ? '' : playListName!,
                textAlign: TextAlign.center,
                style: const TextStyle(color: Colors.white),
              ),
            ],
          ),
        ),
        IconButton(icon: const Icon(Icons.settings, color: Colors.white), onPressed: !fileIdIsValid ? null : () => _showFileOptionsModal(context)),
      ],
    );
  }

  Future<void> _goToPlayList(BuildContext context) async {
    await PlayListPage.forDetails(playListId!, fileId, context);
  }

  void _showFileOptionsModal(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => PlayedFileOptionsBottomSheetDialog(),
    );
  }
}

class _Bottom extends StatelessWidget {
  final int? fileId;
  final int? playListId;
  final String? fileName;

  final bool fileIdIsValid;
  final bool playListIsValid;

  final bool loopFile;
  final bool loopPlayList;
  final bool shufflePlayList;

  const _Bottom({
    this.fileId,
    this.playListId,
    this.fileName,
    required this.fileIdIsValid,
    required this.playListIsValid,
    required this.loopFile,
    required this.loopPlayList,
    required this.shufflePlayList,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = S.of(context);
    return Container(
      margin: const EdgeInsets.symmetric(vertical: 16),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: <Widget>[
          IconButton(
            tooltip: i18n.shufflePlayList,
            icon: Icon(Icons.shuffle, color: shufflePlayList ? theme.colorScheme.primary : Colors.white),
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
            icon: Icon(Icons.repeat, color: loopFile ? theme.colorScheme.primary : Colors.white),
            onPressed: !fileIdIsValid ? null : () => _toggleFileLoop(context),
          ),
        ],
      ),
    );
  }

  Future<void> _togglePlayListShuffle(BuildContext context) =>
      context.read<ServerWsBloc>().setPlayListOptions(playListId!, loop: loopPlayList, shuffle: !shufflePlayList);

  Future<void> _toggleFileLoop(BuildContext context) => context.read<ServerWsBloc>().loopFile(fileId!, playListId!, loop: !loopFile);
}
