import 'package:bloc/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'play_bloc.freezed.dart';
part 'play_event.dart';
part 'play_state.dart';

class PlayBloc extends Bloc<PlayEvent, PlayState> {
  final CastItHubClientService _castItHub;

  bool get isPlaying => state is _PlayingState;

  _PlayingState get currentState => state as _PlayingState;

  PlayBloc(this._castItHub) : super(PlayState.connected()) {
    on<_Connected>((event, emit) {
      final updatedState = PlayState.connected();
      emit(updatedState);
    });

    on<_FileLoading>((event, emit) {
      final updatedState = PlayState.fileLoading();
      emit(updatedState);
    });

    on<_FileLoadingError>((event, emit) {
      final updatedState = PlayState.fileLoadingFailed(msg: event.msg);
      emit(updatedState);
    });

    on<_FileLoaded>((event, emit) {
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

    on<_FileChanged>((event, emit) {
      final updatedState = state.maybeMap(
        playing: (state) => state.copyWith(
          id: event.file.id,
          playListId: event.file.playListId,
          filename: event.file.filename,
          thumbPath: event.file.thumbnailUrl,
          loopFile: event.file.loop,
        ),
        orElse: () => state,
      );
      emit(updatedState);
    });

    on<_PlayListChanged>((event, emit) {
      final updatedState = state.maybeMap(
        playing: (state) => state.copyWith(
          playlistName: event.playList.name,
          playListTotalDuration: event.playList.totalDuration,
          playListPlayedTime: event.playList.playedTime,
          shufflePlayList: event.playList.shuffle,
          loopPlayList: event.playList.loop,
        ),
        orElse: () => state,
      );
      emit(updatedState);
    });

    on<_TimeChanged>(_handleTimeChanged);

    on<_Paused>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      final updatedState = currentState.copyWith.call(isPaused: true);
      emit(updatedState);
    });

    on<_Stopped>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      emit(PlayState.connected());
    });

    on<_Disconnected>((event, emit) => emit(PlayState.connected()));

    on<_SliderDragChanged>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }
      final updatedState = currentState.copyWith.call(isDraggingSlider: event.isSliding);
      emit(updatedState);
    });

    on<_SliderValueChanged>((event, emit) {
      if (!isPlaying) {
        _emitDefaultState(emit);
        return;
      }

      if (event.triggerGoToSeconds) {
        _castItHub.gotoSeconds(event.newValue);
      }

      final updatedState = currentState.copyWith.call(currentSeconds: event.newValue, isDraggingSlider: !event.triggerGoToSeconds);
      emit(updatedState);
    });

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

    _castItHub.disconnected.stream.listen((_) => add(PlayEvent.disconnected()));
  }

  void _handleTimeChanged(_TimeChanged event, Emitter<PlayState> emit) {
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
    emit(PlayState.connected());
  }
}
