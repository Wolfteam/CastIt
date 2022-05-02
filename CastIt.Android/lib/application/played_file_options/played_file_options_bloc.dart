import 'dart:async';

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

  @override
  Stream<PlayedFileOptionsState> mapEventToState(PlayedFileOptionsEvent event) async* {
    final s = event.map(
      loaded: (e) async => state.map(
        loaded: (state) => state.copyWith.call(options: e.options),
        closed: (_) => PlayedFileOptionsState.loaded(options: e.options),
      ),
      setFileOption: (e) async {
        await _castItHub.setFileOptions(e.streamIndex, isAudio: e.isAudio, isQuality: e.isQuality, isSubtitle: e.isSubtitle);
        return state;
      },
      volumeChanged: (e) async => state.map(
        loaded: (state) =>
            state.isDraggingVolumeSlider ? state : state.copyWith(volumeLvl: e.volumeLvl * AppConstants.maxVolumeLevel, isMuted: e.isMuted),
        closed: (s) => s,
      ),
      setVolume: (e) async {
        if (e.triggerChange) {
          await _castItHub.setVolume(e.volumeLvl, isMuted: e.isMuted);
        }

        return state.map(
          loaded: (state) => state.copyWith(volumeLvl: e.volumeLvl, isMuted: e.isMuted, isDraggingVolumeSlider: !e.triggerChange),
          closed: (s) => s,
        );
      },
      closeModal: (e) async => const PlayedFileOptionsState.closed(),
      volumeSliderDragStarted: (_) async => state.map(
        loaded: (state) => state.copyWith.call(isDraggingVolumeSlider: true),
        closed: (state) => state,
      ),
    );

    yield await s;
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
