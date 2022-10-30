import 'package:bloc/bloc.dart';
import 'package:castit/domain/app_constants.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:collection/collection.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'played_file_options_bloc.freezed.dart';
part 'played_file_options_event.dart';
part 'played_file_options_state.dart';

const _initialState = PlayedFileOptionsState.loaded(options: []);

class PlayedFileOptionsBloc extends Bloc<PlayedFileOptionsEvent, PlayedFileOptionsState> {
  final CastItHubClientService _castItHub;

  PlayedFileOptionsBloc(this._castItHub) : super(_initialState) {
    on<_Loaded>((event, emit) {
      final updatedState = state.map(
        loaded: (state) => state.copyWith.call(options: event.options),
        closed: (_) => PlayedFileOptionsState.loaded(options: event.options),
      );

      emit(updatedState);
    });

    on<_SetFileOption>((event, emit) async {
      await _castItHub.setFileOptions(event.streamIndex, isAudio: event.isAudio, isQuality: event.isQuality, isSubtitle: event.isSubtitle);
    });

    on<_VolumeChanged>((event, emit) {
      final updatedState = state.map(
        loaded: (state) =>
            state.isDraggingVolumeSlider ? state : state.copyWith(volumeLvl: event.volumeLvl * AppConstants.maxVolumeLevel, isMuted: event.isMuted),
        closed: (s) => s,
      );

      emit(updatedState);
    });

    on<_SetVolume>((event, emit) async {
      if (event.triggerChange) {
        await _castItHub.setVolume(event.volumeLvl, isMuted: event.isMuted);
      }

      final updatedState = state.map(
        loaded: (state) => state.copyWith(volumeLvl: event.volumeLvl, isMuted: event.isMuted, isDraggingVolumeSlider: !event.triggerChange),
        closed: (s) => s,
      );

      emit(updatedState);
    });

    on<_CloseModal>((event, emit) => emit(const PlayedFileOptionsState.closed()));

    on<_VolumeSliderDragStarted>((event, emit) {
      final updatedState = state.map(
        loaded: (state) => state.copyWith.call(isDraggingVolumeSlider: true),
        closed: (state) => state,
      );
      emit(updatedState);
    });

    _castItHub.fileOptionsLoaded.stream.listen((event) {
      final optionsChanged = _hasFileOptionsChanged(event);
      if (optionsChanged) {
        add(PlayedFileOptionsEvent.loaded(options: event));
      }
    });

    _castItHub.volumeLevelChanged.stream.listen((event) {
      state.map(
        loaded: (s) {
          final volumeChanged = (s.volumeLvl - event.volumeLevel).abs() >= 1 || s.isMuted != event.isMuted;
          if (volumeChanged) {
            add(PlayedFileOptionsEvent.volumeChanged(volumeLvl: event.volumeLevel, isMuted: event.isMuted));
          }
        },
        closed: (_) {},
      );
    });

    _castItHub.fileLoading.stream.listen((event) {
      add(PlayedFileOptionsEvent.closeModal());
    });
  }

  bool _hasFileOptionsChanged(List<FileItemOptionsResponseDto> updated) => state.map(
        loaded: (state) {
          final currentOptions = state.options;
          for (final option in updated) {
            final existing = currentOptions.firstWhereOrNull((el) => el.id == option.id);
            if (option == existing) {
              continue;
            }

            return true;
          }

          return false;
        },
        closed: (_) => true,
      );
}
