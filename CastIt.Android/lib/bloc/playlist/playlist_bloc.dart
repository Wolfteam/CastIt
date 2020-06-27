import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../models/dtos/responses/file_response_dto.dart';
import '../../models/dtos/responses/playlist_response_dto.dart';
import '../../services/castit_service.dart';

part 'playlist_bloc.freezed.dart';
part 'playlist_event.dart';
part 'playlist_state.dart';

class PlayListBloc extends Bloc<PlayListEvent, PlayListState> {
  final CastItService _castItService;

  @override
  PlayListState get initialState => PlayListState.loading();

  PlayListBloc(this._castItService);

  @override
  Stream<PlayListState> mapEventToState(
    PlayListEvent event,
  ) async* {
    if (event is PlayListLoadEvent) {
      yield initialState;
      yield* _loadPlayList(event.playList);
    }
  }

  Stream<PlayListState> _loadPlayList(PlayListResponseDto playlist) async* {
    final response = await _castItService.getAllFiles(playlist.id);
    yield PlayListState.loaded(
      playlistId: playlist.id,
      name: playlist.name,
      loop: playlist.loop,
      position: playlist.position,
      shuffle: playlist.shuffle,
      files: response.result,
      loaded: response.succeed,
    );
  }
}
