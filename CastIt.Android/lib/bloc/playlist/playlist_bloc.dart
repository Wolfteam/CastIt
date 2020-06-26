import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/models/dtos/responses/playlist_response_dto.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/widgets.dart';

import '../../models/dtos/responses/file_response_dto.dart';
import '../../services/castit_service.dart';

part 'playlist_event.dart';
part 'playlist_state.dart';

class PlaylistBloc extends Bloc<PlaylistEvent, PlaylistState> {
  final CastItService _castItService;

  @override
  PlaylistState get initialState => PlaylistLoadingState();

  PlaylistBloc(this._castItService);

  @override
  Stream<PlaylistState> mapEventToState(
    PlaylistEvent event,
  ) async* {
    if (event is LoadPlayList) {
      yield initialState;
      yield* _loadPlayList(event.playlist);
    }
  }

  Stream<PlaylistState> _loadPlayList(PlayListResponseDto playlist) async* {
    final response = await _castItService.getAllFiles(playlist.id);
    yield PlayListLoadedState(
        playlistId: playlist.id,
        name: playlist.name,
        loop: playlist.loop,
        position: playlist.position,
        shuffle: playlist.shuffle,
        files: response.result,
        loaded: response.succeed);
  }
}
