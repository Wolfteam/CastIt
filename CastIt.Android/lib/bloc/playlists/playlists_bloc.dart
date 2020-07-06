import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/bloc/server_ws/server_ws_bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../models/dtos/responses/playlist_response_dto.dart';
import '../../services/castit_service.dart';

part 'playlists_bloc.freezed.dart';
part 'playlists_event.dart';
part 'playlists_state.dart';

class PlayListsBloc extends Bloc<PlayListsEvent, PlayListsState> {
  final CastItService _castItService;

  final ServerWsBloc _serverWsBloc;

  PlayListsBloc(this._castItService, this._serverWsBloc);

  @override
  PlayListsState get initialState => PlayListsState.loading();

  PlayListsLoadedState get currentState => state as PlayListsLoadedState;

  @override
  Stream<PlayListsState> mapEventToState(
    PlayListsEvent event,
  ) async* {
    if (event is PlayListsLoadEvent) {
      yield initialState;
      yield* _loadPlayLists();
    }
  }

  Stream<PlayListsState> _loadPlayLists() async* {
    final response = await _castItService.getAllPlayLists();
    yield PlayListsState.loaded(
      loaded: response.succeed,
      reloads: state is PlayListsLoadedState ? currentState.reloads + 1 : 1,
      playlists: response.result,
    );
  }
}
