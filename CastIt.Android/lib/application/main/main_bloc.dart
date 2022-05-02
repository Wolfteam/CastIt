import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/device_info_service.dart';
import 'package:castit/domain/services/locale_service.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'main_bloc.freezed.dart';
part 'main_event.dart';
part 'main_state.dart';

class MainBloc extends Bloc<MainEvent, MainState> {
  final LoggingService _logger;
  final SettingsService _settings;
  final DeviceInfoService _deviceInfoService;
  final LocaleService _localeService;

  _LoadedState get currentState => state as _LoadedState;

  MainBloc(this._logger, this._settings, this._deviceInfoService, this._localeService) : super(MainState.loading());

  @override
  Stream<MainState> mapEventToState(
    MainEvent event,
  ) async* {
    final s = await event.when(
      init: () async {
        return _init();
      },
      themeChanged: (theme) async {
        return _loadThemeData(currentState.appTitle, theme, _settings.accentColor, _settings.language);
      },
      accentColorChanged: (accentColor) async {
        return _loadThemeData(currentState.appTitle, _settings.appTheme, accentColor, _settings.language);
      },
      goToTab: (index) async {
        return currentState.copyWith(currentSelectedTab: index);
      },
      introCompleted: () async {
        _settings.isFirstInstall = false;
        return currentState.copyWith.call(firstInstall: false);
      },
      languageChanged: () async => state.map(
        loading: (s) => s,
        loaded: (s) => s.copyWith.call(language: _localeService.getCurrentLocale()),
      ),
    );

    yield s;
  }

  Future<MainState> _init() async {
    final appSettings = _settings.appSettings;
    return _loadThemeData(_deviceInfoService.appName, appSettings.appTheme, appSettings.accentColor, appSettings.appLanguage);
  }

  MainState _loadThemeData(
    String appTitle,
    AppThemeType theme,
    AppAccentColorType accentColor,
    AppLanguageType language, {
    bool isInitialized = true,
  }) {
    _logger.info(runtimeType, '_init: Is first install = ${_settings.isFirstInstall}');
    return MainState.loaded(
      appTitle: appTitle,
      initialized: isInitialized,
      theme: theme,
      accentColor: accentColor,
      firstInstall: _settings.isFirstInstall,
      language: _localeService.getLocale(language),
    );
  }
}
