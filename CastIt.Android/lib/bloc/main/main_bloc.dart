import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info/package_info.dart';

import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/extensions/app_theme_type_extensions.dart';
import '../../generated/i18n.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'main_bloc.freezed.dart';
part 'main_event.dart';
part 'main_state.dart';

class MainBloc extends Bloc<MainEvent, MainState> {
  final LoggingService _logger;
  final SettingsService _settings;

  @override
  MainState get initialState => MainState.loading();

  MainLoadedState get currentState => state as MainLoadedState;

  MainBloc(this._logger, this._settings);

  @override
  Stream<MainState> mapEventToState(
    MainEvent event,
  ) async* {
    final s = await event.when(init: () async {
      return _init();
    }, themeChanged: (theme) async {
      return _loadThemeData(currentState.appTitle, theme, _settings.accentColor, _settings.language);
    }, accentColorChanged: (accentColor) async {
      return _loadThemeData(currentState.appTitle, _settings.appTheme, accentColor, _settings.language);
    }, goToTab: (index) async {
      return currentState.copyWith(currentSelectedTab: index);
    });

    yield s;
  }

  Future<MainState> _init() async {
    await _settings.init();
    final packageInfo = await PackageInfo.fromPlatform();
    final appSettings = _settings.appSettings;
    return _loadThemeData(packageInfo.appName, appSettings.appTheme, appSettings.accentColor, appSettings.appLanguage);
  }

  MainState _loadThemeData(
    String appTitle,
    AppThemeType theme,
    AppAccentColorType accentColor,
    AppLanguageType language, {
    bool isInitialized = true,
    bool isConnectedToWs = false,
  }) {
    final themeData = accentColor.getThemeData(theme);
    _setLocale(language);

    return MainState.loaded(appTitle: appTitle, initialized: isInitialized, theme: themeData);
  }

  void _setLocale(AppLanguageType language) {
    final locale = I18n.delegate.supportedLocales[language.index];
    I18n.onLocaleChanged(locale);
  }
}
