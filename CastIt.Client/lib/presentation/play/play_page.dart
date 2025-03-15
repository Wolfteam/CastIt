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
        builder:
            (context, state) => Column(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                Flexible(
                  fit: FlexFit.tight,
                  flex: 70,
                  child: switch (state) {
                    PlayStateConnectingState() => const PlayCoverImg(showLoading: true),
                    PlayStateConnectedState() => const PlayCoverImg(),
                    PlayStateFileLoadingState() => const PlayCoverImg(showLoading: true),
                    PlayStateFileLoadingFailedState() => const PlayCoverImg(),
                    final PlayStatePlayingState state => PlayCoverImg(
                      fileId: state.id,
                      playListId: state.playListId,
                      fileName: state.filename,
                      playListName: state.playlistName,
                      thumbUrl: state.thumbPath,
                      loopFile: state.loopFile,
                      loopPlayList: state.loopPlayList,
                      shufflePlayList: state.shufflePlayList,
                    ),
                  },
                ),
                Flexible(
                  flex: 8,
                  fit: FlexFit.tight,
                  child: switch (state) {
                    final PlayStatePlayingState state => PlayProgressBar(
                      duration: state.duration,
                      currentSeconds: state.currentSeconds,
                    ),
                    _ => const PlayProgressBar(),
                  },
                ),
                switch (state) {
                  final PlayStatePlayingState state => Flexible(
                    flex: 3,
                    fit: FlexFit.tight,
                    child: PlayProgressText(currentSeconds: state.currentSeconds, duration: state.duration),
                  ),
                  _ => const SizedBox.shrink(),
                },
                Flexible(
                  flex: switch (state) {
                    PlayStatePlayingState() => 19,
                    _ => 22,
                  },
                  fit: FlexFit.tight,
                  child: switch (state) {
                    final PlayStatePlayingState state => PlayButtons.playing(isPaused: state.isPaused ?? false),
                    PlayStateFileLoadingState() => const PlayButtons.loading(),
                    _ => const PlayButtons.disabled(),
                  },
                ),
              ],
            ),
      ),
    );
  }
}
