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

  IntroBloc(this._settings, this._settingsBloc) : super(const IntroState.loading()) {
    _settingSubscription = _settingsBloc.stream.listen((e) {
      e.maybeMap(
        loaded: (s) {
          if (state is _LoadedState) {
            add(IntroEvent.languageChanged(newLang: s.appLanguage));
          } else {
            add(IntroEvent.load());
          }
        },
        orElse: () {},
      );
    });
  }

  _LoadedState get currentState => state as _LoadedState;

  @override
  Stream<IntroState> mapEventToState(
    IntroEvent event,
  ) async* {
    yield event.map(
      load: (_) => IntroState.loaded(currentCastItUrl: _settings.castItUrl, currentLang: _settings.language),
      changePage: (e) {
        if (!currentState.urlWasSet) {
          return currentState.copyWith.call(page: e.newPage);
        }
        return currentState.copyWith.call(page: e.newPage, urlWasSet: false);
      },
      urlWasSet: (e) {
        //This can happen when the user press the skip button
        if (e.url.isNullEmptyOrWhitespace) {
          _settings.castItUrl = AppConstants.baseCastItUrl;
        }

        return currentState.copyWith.call(urlWasSet: true, currentCastItUrl: _settings.castItUrl);
      },
      languageChanged: (e) => currentState.copyWith.call(currentLang: e.newLang),
    );
  }

  @override
  Future<void> close() async {
    await _settingSubscription.cancel();
    await super.close();
  }
}
