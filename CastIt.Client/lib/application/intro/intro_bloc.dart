import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/application/bloc.dart';
import 'package:castit/domain/app_constants.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'intro_bloc.freezed.dart';
part 'intro_event.dart';
part 'intro_state.dart';

class IntroBloc extends Bloc<IntroEvent, IntroState> {
  final SettingsService _settings;
  final SettingsBloc _settingsBloc;
  late StreamSubscription _settingSubscription;

  IntroStateLoadedState get currentState => state as IntroStateLoadedState;

  IntroBloc(this._settings, this._settingsBloc) : super(const IntroState.loading()) {
    on<IntroEventLoad>(
      (event, emit) => emit(IntroState.loaded(currentCastItUrl: _settings.castItUrl, currentLang: _settings.language)),
    );

    on<IntroEventChangePage>((event, emit) {
      final updatedState =
          !currentState.urlWasSet
              ? currentState.copyWith.call(page: event.newPage)
              : currentState.copyWith.call(page: event.newPage, urlWasSet: false);
      emit(updatedState);
    });

    on<IntroEventUrlSet>((event, emit) {
      //This can happen when the user press the skip button
      if (event.url.isNullEmptyOrWhitespace) {
        _settings.castItUrl = AppConstants.baseCastItUrl;
      }

      final updatedState = currentState.copyWith.call(urlWasSet: true, currentCastItUrl: _settings.castItUrl);
      emit(updatedState);
    });

    on<IntroEventLanguageChanged>((event, emit) => emit(currentState.copyWith.call(currentLang: event.newLang)));

    _settingSubscription = _settingsBloc.stream.listen((e) {
      switch (e) {
        case SettingsStateLoadedState(:final appLanguage):
          if (state is IntroStateLoadedState) {
            add(IntroEvent.languageChanged(newLang: appLanguage));
          } else {
            add(const IntroEvent.load());
          }
        default:
          break;
      }
    });
  }

  @override
  Future<void> close() async {
    await _settingSubscription.cancel();
    await super.close();
  }
}
