import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/play/play_bloc.dart';
import '../widgets/play/play_buttons.dart';
import '../widgets/play/play_cover_img.dart';
import '../widgets/play/play_progress_bar.dart';
import '../widgets/play/play_progress_text.dart';

class PlayPage extends StatefulWidget {
  @override
  _PlayPageState createState() => _PlayPageState();
}

class _PlayPageState extends State<PlayPage> with AutomaticKeepAliveClientMixin<PlayPage> {
  @override
  bool get wantKeepAlive => true;
//TODO: CUANDO SE MUESTRA EL DIALOGO DE CONEXION HAY UN ERROR ACA
  @override
  Widget build(BuildContext context) {
    super.build(context);
    return Scaffold(
      body: BlocBuilder<PlayBloc, PlayState>(
        builder: (ctx, state) => Column(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            ..._buildPage(state),
          ],
        ),
      ),
    );
  }

  List<Widget> _buildPage(PlayState state) {
    final widgets = [
      Flexible(
        fit: FlexFit.tight,
        flex: 70,
        child: _buildCoverImg(state),
      ),
      const Flexible(flex: 8, fit: FlexFit.tight, child: PlayProgressBar()),
      const Flexible(flex: 3, fit: FlexFit.tight, child: PlayProgressText()),
      Flexible(flex: 19, fit: FlexFit.tight, child: PlayButtons(areDisabled: state is! PlayingState)),
    ];
    return widgets;
  }

  Widget _buildCoverImg(PlayState state) {
    return state.map(
      connecting: (state) => const PlayCoverImg(showLoading: true),
      connected: (state) => const PlayCoverImg(),
      fileLoading: (state) => const PlayCoverImg(showLoading: true),
      fileLoadingFailed: (state) => const PlayCoverImg(),
      playing: (state) => PlayCoverImg(
        fileId: state.id,
        playListId: state.playListId,
        fileName: state.filename,
        playListName: state.playlistName,
        thumbUrl: state.thumbPath,
      ),
    );
  }
}
