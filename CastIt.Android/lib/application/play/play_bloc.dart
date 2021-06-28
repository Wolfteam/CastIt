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
        loopPlayList: file.loopPlayList,
        shufflePlayList: file.shufflePlayList,
      ));
    });

    _serverWsBloc.filePaused.stream.listen((_) {
      if (isPlaying && !currentState.isPaused!) {
        add(PlayEvent.paused());
      }
    });

    _serverWsBloc.fileEndReached.stream.listen((_) {
      add(PlayEvent.stopped());
    });

    _serverWsBloc.fileTimeChanged.stream.listen((seconds) {
      add(PlayEvent.timeChanged(seconds: seconds!));
    });

    _serverWsBloc.disconnected.stream.listen((_) {
      add(PlayEvent.disconnected());
    });
  }

  bool get isPlaying => state is _PlayingState;
  _PlayingState get currentState => state as _PlayingState;

  @override
  Stream<PlayState> mapEventToState(
    PlayEvent event,
  ) async* {
    final s = event.map(
      connected: (_) => PlayState.connected(),
      fileLoading: (_) => PlayState.fileLoading(),
      fileLoadingError: (e) => PlayState.fileLoadingFailed(msg: e.msg),
      fileLoaded: (e) {
        return PlayState.playing(
          id: e.id,
          playListId: e.playListId,
          filename: e.filename,
          thumbPath: e.thumbPath,
          duration: e.duration,
          isPaused: e.isPaused,
          currentSeconds: e.currentSeconds,
          playlistName: e.playlistName,
          loopFile: e.loopFile,
          loopPlayList: e.loopPlayList,
          shufflePlayList: e.shufflePlayList,
          isDraggingSlider: false,
        );
      },
      timeChanged: (e) {
        if (!isPlaying) return null;
        if (currentState.isDraggingSlider!) {
          return currentState.copyWith.call(isPaused: false);
        }
        //A live stream is being played
        if (currentState.duration! <= 0) {
          return currentState.copyWith.call(currentSeconds: e.seconds, isPaused: false);
        }
        final s = e.seconds >= currentState.duration! ? currentState.duration : e.seconds;
        return currentState.copyWith.call(currentSeconds: s, isPaused: false);
      },
      paused: (_) {
        if (!isPlaying) return null;
        return currentState.copyWith.call(isPaused: true);
      },
      stopped: (_) {
        if (!isPlaying) return null;
        return PlayState.connected();
      },
      disconnected: (_) {
        if (!isPlaying) return null;
        return PlayState.connected();
      },
      sliderDragChanged: (e) {
        if (!isPlaying) return null;
        return currentState.copyWith.call(isDraggingSlider: e.isSliding);
      },
      sliderValueChanged: (e) {
        if (!isPlaying) return null;

        if (e.triggerGoToSeconds) {
          _serverWsBloc.gotoSeconds(e.newValue);
        }

        return currentState.copyWith.call(currentSeconds: e.newValue, isDraggingSlider: !e.triggerGoToSeconds);
      },
    );

    if (s != null) {
      yield s;
    } else {
      yield PlayState.connected();
    }
  }
}
