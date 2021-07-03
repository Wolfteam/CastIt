import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/application/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'played_play_list_item_bloc.freezed.dart';
part 'played_play_list_item_event.dart';
part 'played_play_list_item_state.dart';

class PlayedPlayListItemBloc extends Bloc<PlayedPlayListItemEvent, PlayedPlayListItemState> {
  final ServerWsBloc _serverWsBloc;

  PlayedPlayListItemBloc(this._serverWsBloc) : super(const PlayedPlayListItemState.notPlaying()) {
    _serverWsBloc.fileLoaded.stream.listen((file) {
      add(PlayedPlayListItemEvent.playing(id: file.playListId, totalDuration: file.playListTotalDuration!));
    });

    _serverWsBloc.fileEndReached.stream.listen((file) {
      add(PlayedPlayListItemEvent.endReached());
    });
  }

  @override
  Stream<PlayedPlayListItemState> mapEventToState(PlayedPlayListItemEvent event) async* {
    final s = event.map(
      playing: (e) => PlayedPlayListItemState.playing(id: e.id, totalDuration: e.totalDuration),
      endReached: (_) => const PlayedPlayListItemState.notPlaying(),
    );

    yield s;
  }
}
