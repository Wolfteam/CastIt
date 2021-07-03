import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/application/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'played_file_item_bloc.freezed.dart';
part 'played_file_item_event.dart';
part 'played_file_item_state.dart';

class PlayedFileItemBloc extends Bloc<PlayedFileItemEvent, PlayedFileItemState> {
  final ServerWsBloc _serverWsBloc;

  PlayedFileItemBloc(this._serverWsBloc) : super(const PlayedFileItemState.notPlaying()) {
    _serverWsBloc.fileChanged.stream.listen((event) {
      final file = event.item2;
      add(PlayedFileItemEvent.playing(
        id: file.id,
        playListId: file.playListId,
        playedPercentage: file.playedPercentage,
        fullTotalDuration: file.fullTotalDuration,
      ));
    });
  }

  @override
  Stream<PlayedFileItemState> mapEventToState(PlayedFileItemEvent event) async* {
    final s = event.map(
      playing: (e) => PlayedFileItemState.playing(
        id: e.id,
        playListId: e.playListId,
        playedPercentage: e.playedPercentage,
        fullTotalDuration: e.fullTotalDuration,
      ),
      endReached: (_) => const PlayedFileItemState.notPlaying(),
    );

    yield s;
  }
}
