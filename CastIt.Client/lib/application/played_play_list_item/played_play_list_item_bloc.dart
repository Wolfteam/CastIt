import 'package:bloc/bloc.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'played_play_list_item_bloc.freezed.dart';
part 'played_play_list_item_event.dart';
part 'played_play_list_item_state.dart';

class PlayedPlayListItemBloc extends Bloc<PlayedPlayListItemEvent, PlayedPlayListItemState> {
  final CastItHubClientService _castItHub;

  PlayedPlayListItemBloc(this._castItHub) : super(const PlayedPlayListItemState.notPlaying()) {
    on<PlayedPlayListItemEventPlaying>(
      (event, emit) => emit(PlayedPlayListItemState.playing(id: event.id, totalDuration: event.totalDuration)),
    );

    on<PlayedPlayListItemEventEndReached>((event, emit) => emit(const PlayedPlayListItemState.notPlaying()));

    _castItHub.fileLoaded.stream.listen((file) {
      add(PlayedPlayListItemEvent.playing(id: file.playListId, totalDuration: file.playListTotalDuration!));
    });

    _castItHub.fileEndReached.stream.listen((file) {
      add(const PlayedPlayListItemEvent.endReached());
    });
  }
}
