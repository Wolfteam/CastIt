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
  PlayedFileOptionsBloc(this._serverWsBloc) {
    _serverWsBloc.fileOptionsLoaded.stream.listen((event) {
      add(PlayedFileOptionsEvent.loaded(options: event));
    });
  }

  @override
  PlayedFileOptionsState get initialState => PlayedFileOptionsState.loading();
  @override
  Stream<PlayedFileOptionsState> mapEventToState(
    PlayedFileOptionsEvent event,
  ) async* {
    final s = event.map(
      load: (e) async {
        await _serverWsBloc.loadFileOptions(e.id);
        return initialState;
      },
      loaded: (e) async => PlayedFileOptionsState.loaded(options: e.options),
    );

    yield await s;
  }
}
