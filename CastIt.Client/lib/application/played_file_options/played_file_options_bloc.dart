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
    on<PlayedFileOptionsEventLoaded>((event, emit) {
      final updatedState = switch (state) {
        final PlayedFileOptionsStateLoadedState state => state.copyWith.call(options: event.options),
        PlayedFileOptionsStateClosedState() => PlayedFileOptionsState.loaded(options: event.options),
      };

      emit(updatedState);
    });

    on<PlayedFileOptionsEventSetFileOption>((event, emit) async {
      await _castItHub.setFileOptions(
        event.streamIndex,
        isAudio: event.isAudio,
        isQuality: event.isQuality,
        isSubtitle: event.isSubtitle,
      );
    });

    on<PlayedFileOptionsEventVolumeChanged>((event, emit) {
      final updatedState = switch (state) {
        final PlayedFileOptionsStateLoadedState state =>
          state.isDraggingVolumeSlider
              ? state
              : state.copyWith(volumeLvl: event.volumeLvl * AppConstants.maxVolumeLevel, isMuted: event.isMuted),
        PlayedFileOptionsStateClosedState() => state,
      };

      emit(updatedState);
    });

    on<PlayedFileOptionsEventSetVolume>((event, emit) async {
      if (event.triggerChange) {
        await _castItHub.setVolume(event.volumeLvl, isMuted: event.isMuted);
      }

      final updatedState = switch (state) {
        final PlayedFileOptionsStateLoadedState state => state.copyWith(
          volumeLvl: event.volumeLvl,
          isMuted: event.isMuted,
          isDraggingVolumeSlider: !event.triggerChange,
        ),
        PlayedFileOptionsStateClosedState() => state,
      };

      emit(updatedState);
    });

    on<PlayedFileOptionsEventCloseModal>((event, emit) => emit(const PlayedFileOptionsState.closed()));

    on<PlayedFileOptionsEventVolumeSliderDragStarted>((event, emit) {
      final updatedState = switch (state) {
        final PlayedFileOptionsStateLoadedState state => state.copyWith.call(isDraggingVolumeSlider: true),
        PlayedFileOptionsStateClosedState() => state,
      };
      emit(updatedState);
    });

    _castItHub.fileOptionsLoaded.stream.listen((event) {
      final optionsChanged = _hasFileOptionsChanged(event);
      if (optionsChanged) {
        add(PlayedFileOptionsEvent.loaded(options: event));
      }
    });

    _castItHub.volumeLevelChanged.stream.listen((event) {
      switch (state) {
        case final PlayedFileOptionsStateLoadedState state:
          final volumeChanged = (state.volumeLvl - event.volumeLevel).abs() >= 1 || state.isMuted != event.isMuted;
          if (volumeChanged) {
            add(PlayedFileOptionsEvent.volumeChanged(volumeLvl: event.volumeLevel, isMuted: event.isMuted));
          }
        default:
          break;
      }
    });

    _castItHub.fileLoading.stream.listen((event) {
      add(const PlayedFileOptionsEvent.closeModal());
    });
  }

  bool _hasFileOptionsChanged(List<FileItemOptionsResponseDto> updated) {
    switch (state) {
      case final PlayedFileOptionsStateLoadedState state:
        final List<FileItemOptionsResponseDto> currentOptions = state.options;
        for (final FileItemOptionsResponseDto option in updated) {
          final FileItemOptionsResponseDto? existing = currentOptions.firstWhereOrNull((el) => el.id == option.id);
          if (option == existing) {
            continue;
          }
          return true;
        }
        return false;
      case PlayedFileOptionsStateClosedState():
        return true;
    }
  }
}
