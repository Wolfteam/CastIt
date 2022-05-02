import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'play_bloc.freezed.dart';
part 'play_event.dart';
part 'play_state.dart';

class PlayBloc extends Bloc<PlayEvent, PlayState> {
  final CastItHubClientService _castItHub;

  PlayBloc(this._castItHub) : super(PlayState.connected()) {
    _castItHub.connected.stream.listen((_) => add(PlayEvent.connected()));

    _castItHub.fileLoading.stream.listen((_) => add(PlayEvent.fileLoading()));

    _castItHub.fileLoadingError.stream.listen((msg) => add(PlayEvent.fileLoadingError(msg: msg)));

    _castItHub.fileLoaded.stream.listen((file) {
      if (isPlaying && currentState.id == file.id) {
        return;
      }
      add(PlayEvent.fileLoaded(file: file));
    });

    _castItHub.filePaused.stream.listen((_) {
      if (isPlaying && !currentState.isPaused!) {
        add(PlayEvent.paused());
      }
    });

    _castItHub.fileEndReached.stream.listen((_) {
      add(PlayEvent.stopped());
    });

    _castItHub.fileTimeChanged.stream.listen((seconds) {
      if (isPlaying && (currentState.currentSeconds! - seconds).abs() >= 1) {
        add(PlayEvent.timeChanged(seconds: seconds));
      }
    });

    _castItHub.disconnected.stream.listen((_) => add(PlayEvent.disconnected()));
  }

  bool get isPlaying => state is _PlayingState;

  _PlayingState get currentState => state as _PlayingState;

  @override
  Stream<PlayState> mapEventToState(PlayEvent event) async* {
    final s = event.map(
      connected: (_) => PlayState.connected(),
      fileLoading: (_) => PlayState.fileLoading(),
      fileLoadingError: (e) => PlayState.fileLoadingFailed(msg: e.msg),
      fileLoaded: (e) => PlayState.playing(
        id: e.file.id,
        playListId: e.file.playListId,
        filename: e.file.filename,
        thumbPath: e.file.thumbnailUrl,
        duration: e.file.duration,
        isPaused: e.file.isPaused,
        currentSeconds: e.file.currentSeconds,
        playlistName: e.file.playListName,
        loopFile: e.file.loopFile,
        loopPlayList: e.file.loopPlayList,
        shufflePlayList: e.file.shufflePlayList,
        isDraggingSlider: false,
        playListTotalDuration: e.file.playListTotalDuration,
        playListPlayedTime: e.file.playListPlayedTime,
      ),
      timeChanged: (e) {
        if (!isPlaying) {
          return null;
        }
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
        if (!isPlaying) {
          return null;
        }
        return currentState.copyWith.call(isPaused: true);
      },
      stopped: (_) {
        if (!isPlaying) {
          return null;
        }
        return PlayState.connected();
      },
      disconnected: (_) {
        if (!isPlaying) {
          return null;
        }
        return PlayState.connected();
      },
      sliderDragChanged: (e) {
        if (!isPlaying) {
          return null;
        }
        return currentState.copyWith.call(isDraggingSlider: e.isSliding);
      },
      sliderValueChanged: (e) {
        if (!isPlaying) {
          return null;
        }

        if (e.triggerGoToSeconds) {
          _castItHub.gotoSeconds(e.newValue);
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
