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

  MainStateLoadedState get currentState => state as MainStateLoadedState;

  MainBloc(this._logger, this._settings, this._deviceInfoService, this._localeService) : super(const MainState.loading()) {
    on<MainEventInit>((event, emit) async {
      final updatedState = await _init();
      emit(updatedState);
    });

    on<MainEventThemeChanged>((event, emit) {
      final updatedState = _loadThemeData(currentState.appTitle, event.theme, _settings.accentColor, _settings.language);
      emit(updatedState);
    });

    on<MainEventAccentColorChanged>((event, emit) {
      final updatedState = _loadThemeData(currentState.appTitle, _settings.appTheme, event.accentColor, _settings.language);
      emit(updatedState);
    });

    on<MainEventGoToTab>((event, emit) {
      final updatedState = currentState.copyWith(currentSelectedTab: event.index);
      emit(updatedState);
    });

    on<MainEventIntroCompleted>((event, emit) {
      _settings.isFirstInstall = false;
      final updatedState = currentState.copyWith.call(firstInstall: false);
      emit(updatedState);
    });

    on<MainEventLanguageChanged>((event, emit) {
      final updatedState = switch (state) {
        final MainStateLoadedState state => state.copyWith.call(language: _localeService.getCurrentLocale()),
        _ => state,
      };
      emit(updatedState);
    });
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
