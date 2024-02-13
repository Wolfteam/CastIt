import 'package:castit/application/bloc.dart';
import 'package:castit/presentation/play/widgets/play_buttons.dart';
import 'package:castit/presentation/play/widgets/play_cover_img.dart';
import 'package:castit/presentation/play/widgets/play_progress_bar.dart';
import 'package:castit/presentation/play/widgets/play_progress_text.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayPage extends StatefulWidget {
  @override
  _PlayPageState createState() => _PlayPageState();
}

class _PlayPageState extends State<PlayPage> with AutomaticKeepAliveClientMixin<PlayPage> {
  @override
  bool get wantKeepAlive => true;

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return Scaffold(
      body: BlocBuilder<PlayBloc, PlayState>(
        builder: (context, state) => Column(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            Flexible(
              fit: FlexFit.tight,
              flex: 70,
              child: state.map(
                connecting: (s) => const PlayCoverImg(showLoading: true),
                connected: (s) => const PlayCoverImg(),
                fileLoading: (s) => const PlayCoverImg(showLoading: true),
                fileLoadingFailed: (s) => const PlayCoverImg(),
                playing: (s) => PlayCoverImg(
                  fileId: s.id,
                  playListId: s.playListId,
                  fileName: s.filename,
                  playListName: s.playlistName,
                  thumbUrl: s.thumbPath,
                  loopFile: s.loopFile,
                  loopPlayList: s.loopPlayList,
                  shufflePlayList: s.shufflePlayList,
                ),
              ),
            ),
            Flexible(
              flex: 8,
              fit: FlexFit.tight,
              child: state.maybeMap(
                playing: (state) => PlayProgressBar(duration: state.duration, currentSeconds: state.currentSeconds),
                orElse: () => const PlayProgressBar(),
              ),
            ),
            state.maybeMap(
              playing: (state) => Flexible(
                flex: 3,
                fit: FlexFit.tight,
                child: PlayProgressText(
                  currentSeconds: state.currentSeconds,
                  duration: state.duration,
                ),
              ),
              orElse: () => const SizedBox.shrink(),
            ),
            Flexible(
              flex: state.maybeMap(playing: (_) => 19, orElse: () => 22),
              fit: FlexFit.tight,
              child: state.maybeMap(
                fileLoading: (state) => const PlayButtons.loading(),
                playing: (state) => PlayButtons.playing(isPaused: state.isPaused ?? false),
                orElse: () => const PlayButtons.disabled(),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
