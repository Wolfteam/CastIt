import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:meta/meta.dart';

import '../server_ws/server_ws_bloc.dart';

part 'play_bloc.freezed.dart';
part 'play_event.dart';
part 'play_state.dart';

class PlayBloc extends Bloc<PlayEvent, PlayState> {
  final ServerWsBloc _serverWsBloc;

  PlayBloc(this._serverWsBloc) : super(PlayState.connected()) {
    _serverWsBloc.connected.stream.listen((_) {
      add(PlayEvent.connected());
    });

    _serverWsBloc.fileLoading.stream.listen((_) {
      add(PlayEvent.fileLoading());
    });

    _serverWsBloc.fileLoadingError.stream.listen((msg) {
      add(PlayEvent.fileLoadingError(msg: msg));
    });

    _serverWsBloc.fileLoaded.stream.listen((file) {
      add(PlayEvent.fileLoaded(
        id: file.id,
        filename: file.filename,
        thumbPath: file.thumbnailUrl,
        duration: file.duration,
        loopFile: file.loopFile,
        currentSeconds: file.currentSeconds,
        isPaused: file.isPaused,
        volumeLevel: file.volumeLevel,
        isMuted: file.isMuted,
        playListId: file.playListId,
        playlistName: file.playListName,
        shufflePlayList: file.shufflePlayList,
      ));
    });

    _serverWsBloc.filePaused.stream.listen((_) {
      add(PlayEvent.paused());
    });

    _serverWsBloc.fileEndReached.stream.listen((_) {
      add(PlayEvent.stopped());
    });

    _serverWsBloc.fileTimeChanged.stream.listen((seconds) {
      add(PlayEvent.timeChanged(seconds: seconds));
    });

    _serverWsBloc.disconnected.stream.listen((_) {
      add(PlayEvent.disconnected());
    });
  }

  bool get isPlaying => state is PlayingState;
  PlayingState get currentState => state as PlayingState;

  @override
  Stream<PlayState> mapEventToState(
    PlayEvent event,
  ) async* {
    final s = event.when(
      connected: () => PlayState.connected(),
      fileLoading: () => PlayState.fileLoading(),
      fileLoadingError: (msg) => PlayState.fileLoadingFailed(msg: msg),
      fileLoaded: (
        id,
        title,
        thumbPath,
        duration,
        loop,
        currentSeconds,
        isPaused,
        volumeLvl,
        isMuted,
        playListId,
        playlistName,
        shuffle,
      ) {
        return PlayState.playing(
          id: id,
          playListId: playListId,
          filename: title,
          thumbPath: thumbPath,
          duration: duration,
          isPaused: isPaused,
          currentSeconds: currentSeconds,
          playlistName: playlistName,
          loopFile: loop,
          shufflePlayList: shuffle,
        );
      },
      timeChanged: (seconds) {
        if (!isPlaying) return null;
        final s = seconds >= currentState.duration ? currentState.duration : seconds;
        return currentState.copyWith.call(currentSeconds: s, isPaused: false);
      },
      paused: () {
        if (!isPlaying) return null;
        return currentState.copyWith.call(isPaused: true);
      },
      stopped: () {
        if (!isPlaying) return null;
        return PlayState.connected();
      },
      volumeLvlChanged: (volumeLvl, isPaused) {
        return null;
      },
      disconnected: () {
        if (!isPlaying) return null;
        return PlayState.connected();
      },
    );

    if (s != null) {
      yield s;
    } else {
      yield PlayState.connected();
    }
  }
}
