import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../common/app_constants.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/extensions/string_extensions.dart';
import '../../services/settings_service.dart';
import '../settings/settings_bloc.dart';

part 'intro_bloc.freezed.dart';
part 'intro_event.dart';
part 'intro_state.dart';

class IntroBloc extends Bloc<IntroEvent, IntroState> {
  final SettingsService _settings;
  final SettingsBloc _settingsBloc;
  StreamSubscription _settingSubscription;

  IntroBloc(this._settings, this._settingsBloc) : super(IntroState.loading()) {
    _settingSubscription = _settingsBloc.stream.listen((e) {
      if (e is SettingsLoadedState && state is IntroLoadedState) {
        add(IntroEvent.languageChanged(newLang: e.appLanguage));
      } else if (e is SettingsLoadedState) {
        add(IntroEvent.load());
      }
    });
  }

  IntroLoadedState get currentState => state as IntroLoadedState;

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
