import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/generated/i18n.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info/package_info.dart';

import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'settings_bloc.freezed.dart';
part 'settings_event.dart';
part 'settings_state.dart';

class SettingsBloc extends Bloc<SettingsEvent, SettingsState> {
  final LoggingService _logger;
  final SettingsService _settings;

  SettingsBloc(this._logger, this._settings);

  @override
  SettingsState get initialState => SettingsState.loading();

  SettingsLoadedState get currentState => state as SettingsLoadedState;

  @override
  Stream<SettingsState> mapEventToState(
    SettingsEvent event,
  ) async* {
    if (event is SettingsLoadEvent) {
      await _settings.init();
      final packageInfo = await PackageInfo.fromPlatform();
      final settings = _settings.appSettings;
      yield SettingsState.loaded(
        appTheme: settings.appTheme,
        useDarkAmoled: settings.useDarkAmoled,
        accentColor: settings.accentColor,
        appLanguage: settings.appLanguage,
        castItUrl: settings.castItUrl,
        appName: packageInfo.appName,
        appVersion: packageInfo.version,
      );
    }

    if (event is SettingsThemeChangedEvent) {
      _settings.appTheme = event.theme;
      yield currentState.copyWith(appTheme: event.theme);
    }

    if (event is SettingsAccentColorChangedEvent) {
      _settings.accentColor = event.accentColor;
      yield currentState.copyWith(accentColor: event.accentColor);
    }

    if (event is SettingsLanguageChangedEvent) {
      _settings.language = event.lang;
      final locale = I18n.delegate.supportedLocales[event.lang.index];
      I18n.onLocaleChanged(locale);
      yield currentState.copyWith(appLanguage: event.lang);
    }
  }
}
