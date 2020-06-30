import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:meta/meta.dart';

import 'package:castit/bloc/main/main_bloc.dart';

part 'play_bloc.freezed.dart';
part 'play_event.dart';
part 'play_state.dart';

class PlayBloc extends Bloc<PlayEvent, PlayState> {
  final MainBloc _mainBloc;
  PlayBloc(this._mainBloc) {
    _mainBloc.connected.stream.listen((_) {
      add(PlayEvent.connected());
    });

    _mainBloc.fileLoading.stream.listen((_) {
      add(PlayEvent.fileLoading());
    });

    _mainBloc.fileLoadingError.stream.listen((msg) {
      add(PlayEvent.fileLoadingError(msg: msg));
    });

    _mainBloc.fileLoaded.stream.listen((file) {
      add(PlayEvent.fileLoaded(
        filename: file.filename,
        thumbPath: file.thumbnailUrl,
        duration: file.duration,
        loopFile: file.loopFile,
        currentSeconds: file.currentSeconds,
        isPaused: file.isPaused,
        volumeLevel: file.volumeLevel,
        isMuted: file.isMuted,
        playlistName: file.playListName,
        shufflePlayList: file.shufflePlayList,
      ));
    });

    _mainBloc.filePaused.stream.listen((_) {
      add(PlayEvent.paused());
    });

    _mainBloc.fileEndReached.stream.listen((_) {
      add(PlayEvent.stopped());
    });

    _mainBloc.fileTimeChanged.stream.listen((seconds) {
      add(PlayEvent.timeChanged(seconds: seconds));
    });

    _mainBloc.disconnected.stream.listen((_) {
      add(PlayEvent.disconnected());
    });
  }

  @override
  PlayState get initialState => PlayState.connected();

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
        title,
        thumbPath,
        duration,
        loop,
        currentSeconds,
        isPaused,
        volumeLvl,
        isMuted,
        playlistName,
        shuffle,
      ) {
        return PlayState.playing(
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
        return currentState.copyWith.call(currentSeconds: s);
      },
      paused: () {
        if (!isPlaying) return null;
        return currentState.copyWith.call(isPaused: !currentState.isPaused);
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
      yield initialState;
    }
  }
}
