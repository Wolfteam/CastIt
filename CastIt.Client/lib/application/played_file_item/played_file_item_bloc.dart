import 'package:bloc/bloc.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'played_file_item_bloc.freezed.dart';
part 'played_file_item_event.dart';
part 'played_file_item_state.dart';

class PlayedFileItemBloc extends Bloc<PlayedFileItemEvent, PlayedFileItemState> {
  final CastItHubClientService _castItHub;

  PlayedFileItemBloc(this._castItHub) : super(const PlayedFileItemState.notPlaying()) {
    on<_Playing>((event, emit) {
      final updatedState = PlayedFileItemState.playing(
        id: event.id,
        playListId: event.playListId,
        playedPercentage: event.playedPercentage,
        fullTotalDuration: event.fullTotalDuration,
      );
      emit(updatedState);
    });

    on<_EndReached>((event, emit) => emit(const PlayedFileItemState.notPlaying()));

    _castItHub.fileChanged.stream.listen((event) {
      final file = event.item2;
      add(
        PlayedFileItemEvent.playing(
          id: file.id,
          playListId: file.playListId,
          playedPercentage: file.playedPercentage,
          fullTotalDuration: file.fullTotalDuration,
        ),
      );
    });
  }
}
