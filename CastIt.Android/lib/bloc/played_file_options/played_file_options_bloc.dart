import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:meta/meta.dart';

import '../../models/dtos/responses/file_item_options_response_dto.dart';
import '../server_ws/server_ws_bloc.dart';

part 'played_file_options_bloc.freezed.dart';
part 'played_file_options_event.dart';
part 'played_file_options_state.dart';

class PlayedFileOptionsBloc extends Bloc<PlayedFileOptionsEvent, PlayedFileOptionsState> {
  final ServerWsBloc _serverWsBloc;

  PlayedFileOptionsState get initialState => PlayedFileOptionsState.loading();

  PlayedFileOptionsLoadedState get currentState => state as PlayedFileOptionsLoadedState;

  PlayedFileOptionsBloc(this._serverWsBloc) : super(PlayedFileOptionsState.loading()) {
    _serverWsBloc.fileOptionsLoaded.stream.listen((event) {
      add(PlayedFileOptionsEvent.loaded(options: event));
    });

    _serverWsBloc.volumeLevelChanged.stream.listen((event) {
      add(PlayedFileOptionsEvent.volumeChanged(volumeLvl: event.volumeLevel, isMuted: event.isMuted));
    });

    _serverWsBloc.fileLoading.stream.listen((event) {
      add(PlayedFileOptionsEvent.closeModal());
    });
  }

  @override
  Stream<PlayedFileOptionsState> mapEventToState(
    PlayedFileOptionsEvent event,
  ) async* {
    if (event is PlayedFileOptionsVolumeLevelChangedEvent && state is! PlayedFileOptionsLoadedState) {
      return;
    }

    final s = event.map(
      load: (e) async {
        await _serverWsBloc.loadFileOptions(e.id);
        return initialState;
      },
      loaded: (e) async => PlayedFileOptionsState.loaded(options: e.options),
      setFileOption: (e) async {
        await _serverWsBloc.setFileOptions(
          e.streamIndex,
          isAudio: e.isAudio,
          isQuality: e.isQuality,
          isSubtitle: e.isSubtitle,
        );
        return initialState;
      },
      volumeChanged: (e) async {
        return currentState.copyWith(volumeLvl: e.volumeLvl, isMuted: e.isMuted);
      },
      setVolume: (e) async {
        await _serverWsBloc.setVolume(e.volumeLvl, isMuted: e.isMuted);
        return currentState.copyWith(volumeLvl: e.volumeLvl, isMuted: e.isMuted);
      },
      closeModal: (e) async => PlayedFileOptionsState.closed(),
    );

    yield await s;
  }
}
