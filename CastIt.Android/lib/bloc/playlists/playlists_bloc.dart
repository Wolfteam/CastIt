import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/widgets.dart';

import '../../models/dtos/responses/playlist_response_dto.dart';
import '../../services/castit_service.dart';

part 'playlists_event.dart';
part 'playlists_state.dart';

class PlaylistsBloc extends Bloc<PlaylistsEvent, PlaylistsState> {
  final CastItService _castItService;

  PlaylistsBloc(
    this._castItService,
  );

  @override
  PlaylistsState get initialState => PlayListsLoadingState();

  PlayListsLoadedState get currentState => state as PlayListsLoadedState;

  @override
  Stream<PlaylistsState> mapEventToState(
    PlaylistsEvent event,
  ) async* {
    if (event is LoadPlayLists) {
      yield initialState;
      yield* _loadPlayLists();
    }
  }

  Stream<PlaylistsState> _loadPlayLists() async* {
    final response = await _castItService.getAllPlayLists();
    yield PlayListsLoadedState(
      loaded: response.succeed,
      reloads: state is PlayListsLoadedState ? currentState.reloads + 1 : 1,
      playlists: response.result,
    );
  }
}
