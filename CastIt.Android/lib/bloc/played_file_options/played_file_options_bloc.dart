import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:meta/meta.dart';

import '../../models/dtos/responses/file_item_options_response_dto.dart';
import '../server_ws/server_ws_bloc.dart';

part 'played_file_options_bloc.freezed.dart';
part 'played_file_options_event.dart';
part 'played_file_options_state.dart';

const _initialState = PlayedFileOptionsState.loaded(options: []);

class PlayedFileOptionsBloc extends Bloc<PlayedFileOptionsEvent, PlayedFileOptionsState> {
  final ServerWsBloc _serverWsBloc;

  PlayedFileOptionsBloc(this._serverWsBloc) : super(_initialState) {
    _serverWsBloc.fileOptionsLoaded.stream.listen((event) {
      add(PlayedFileOptionsEvent.loaded(options: event));
    });

    _serverWsBloc.volumeLevelChanged.stream.listen((event) {
      add(PlayedFileOptionsEvent.volumeChanged(volumeLvl: event!.volumeLevel, isMuted: event.isMuted));
    });

    _serverWsBloc.fileLoading.stream.listen((event) {
      add(PlayedFileOptionsEvent.closeModal());
    });
  }

  @override
  Stream<PlayedFileOptionsState> mapEventToState(
    PlayedFileOptionsEvent event,
  ) async* {
    final s = event.map(
      loaded: (e) async => PlayedFileOptionsState.loaded(options: e.options),
      setFileOption: (e) async {
        await _serverWsBloc.setFileOptions(
          e.streamIndex,
          isAudio: e.isAudio,
          isQuality: e.isQuality,
          isSubtitle: e.isSubtitle,
        );
        return state;
      },
      volumeChanged: (e) async => state.map(
        loaded: (state) => state.copyWith(volumeLvl: e.volumeLvl, isMuted: e.isMuted),
        closed: (s) => s,
      ),
      setVolume: (e) async {
        await _serverWsBloc.setVolume(e.volumeLvl, isMuted: e.isMuted);
        return state.map(
          loaded: (state) => state.copyWith(volumeLvl: e.volumeLvl, isMuted: e.isMuted),
          closed: (s) => s,
        );
      },
      closeModal: (e) async => _initialState,
    );

    yield await s;
  }
}
