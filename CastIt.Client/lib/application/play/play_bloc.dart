import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'play_bloc.freezed.dart';
part 'play_event.dart';
part 'play_state.dart';

class PlayBloc extends Bloc<PlayEvent, PlayState> {
  final CastItHubClientService _castItHub;

  bool get isPlaying => state is PlayStatePlayingState;

  PlayStatePlayingState get currentState => state as PlayStatePlayingState;

  PlayBloc(this._castItHub) : super(const PlayState.connected()) {
    on<PlayEventConnected>((event, emit) {
      const updatedState = PlayState.connected();
      emit(updatedState);
    });

    on<PlayEventFileLoading>((event, emit) {
      const updatedState = PlayState.fileLoading();
      emit(updatedState);
    });

    on<PlayEventFileLoadingError>((event, emit) {
      final updatedState = PlayState.fileLoadingFailed(msg: event.msg);
      emit(updatedState);
    });

    on<PlayEventFileLoaded>((event, emit) {
      final updatedState = PlayState.playing(
        id: event.file.id,
        playListId: event.file.playListId,
        filename: event.file.filename,
        thumbPath: event.file.thumbnailUrl,
        duration: event.file.duration,
        isPaused: event.file.isPaused,
        currentSeconds: event.file.currentSeconds,
        playlistName: event.file.playListName,
        loopFile: event.file.loopFile,
        loopPlayList: event.file.loopPlayList,
        shufflePlayList: event.file.shufflePlayList,
        isDraggingSlider: false,
        playListTotalDuration: event.file.playListTotalDuration,
        playListPlayedTime: event.file.playListPlayedTime,
      );
      emit(updatedState);
    });

    on<PlayEventFileChanged>((event, emit) {
      final updatedState = switch (state) {
        final PlayStatePlayingState state => state.copyWith(
          id: event.file.id,
          playListId: event.file.playListId,
          filename: event.file.filename,
          thumbPath: event.file.thumbnailUrl,
          loopFile: event.file.loop,
        ),
        _ => state,
      };

      emit(updatedState);
    });

    on<PlayEventPlayListChanged>((event, emit) {
      final updatedState = switch (state) {
        final PlayStatePlayingState state => state.copyWith(
          playlistName: event.playList.name,
          playListTotalDuration: event.playList.totalDuration,
          playListPlayedTime: event.playList.playedTime,
          shufflePlayList: event.playList.shuffle,
          loopPlayList: event.playList.loop,
        ),
        _ => state,
      };

      emit(updatedState);
    });

    on<PlayEventTimeChanged>(_handleTimeChanged);

    on<PlayEventPaused>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      final updatedState = currentState.copyWith.call(isPaused: true);
      emit(updatedState);
    });

    on<PlayEventStopped>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      emit(const PlayState.connected());
    });

    on<PlayEventDisconnected>((event, emit) => emit(const PlayState.connected()));

    on<PlayEventSliderDragChanged>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      final updatedState = currentState.copyWith.call(isDraggingSlider: event.isSliding);
      emit(updatedState);
    });

    on<PlayEventSliderValueChanged>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }

      if (event.triggerGoToSeconds) {
        _castItHub.gotoSeconds(event.newValue);
      }

      final updatedState = currentState.copyWith.call(
        currentSeconds: event.newValue,
        isDraggingSlider: !event.triggerGoToSeconds,
      );
      emit(updatedState);
    });

    _castItHub.connected.stream.listen((_) => add(const PlayEvent.connected()));

    _castItHub.fileLoading.stream.listen((_) => add(const PlayEvent.fileLoading()));

    _castItHub.fileLoadingError.stream.listen((msg) => add(PlayEvent.fileLoadingError(msg: msg)));

    _castItHub.fileLoaded.stream.listen((file) {
      if (isPlaying && currentState.id == file.id) {
        return;
      }
      add(PlayEvent.fileLoaded(file: file));
    });

    _castItHub.filePaused.stream.listen((_) {
      if (isPlaying && !currentState.isPaused!) {
        add(const PlayEvent.paused());
      }
    });

    _castItHub.fileEndReached.stream.listen((_) {
      add(const PlayEvent.stopped());
    });

    _castItHub.fileChanged.stream.listen((tuple) {
      //if the changed file is not the one being played just return
      if (!tuple.$1) {
        return;
      }
      add(PlayEvent.fileChanged(file: tuple.$2));
    });

    _castItHub.playListChanged.stream.listen((tuple) {
      //if the changed playlist is not the one being played just return
      if (!tuple.$1) {
        return;
      }
      add(PlayEvent.playListChanged(playList: tuple.$2));
    });

    _castItHub.fileTimeChanged.stream.listen((seconds) {
      if (isPlaying && (currentState.currentSeconds! - seconds).abs() >= 1) {
        add(PlayEvent.timeChanged(seconds: seconds));
      }
    });

    _castItHub.disconnected.stream.listen((_) => add(const PlayEvent.disconnected()));
  }

  void _handleTimeChanged(PlayEventTimeChanged event, Emitter<PlayState> emit) {
    if (!isPlaying) {
      _emitDefaultState(emit);
      return;
    }
    if (currentState.isDraggingSlider!) {
      final updatedState = currentState.copyWith.call(isPaused: false);
      emit(updatedState);
      return;
    }
    //A live stream is being played
    if (currentState.duration! <= 0) {
      final updatedState = currentState.copyWith.call(currentSeconds: event.seconds, isPaused: false);
      emit(updatedState);
      return;
    }

    final s = event.seconds >= currentState.duration! ? currentState.duration : event.seconds;
    final updatedState = currentState.copyWith.call(currentSeconds: s, isPaused: false);
    emit(updatedState);
  }

  void _emitDefaultState(Emitter<PlayState> emit) {
    emit(const PlayState.connected());
  }
}
