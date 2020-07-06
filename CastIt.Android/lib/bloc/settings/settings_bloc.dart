import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info/package_info.dart';

import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/enums/video_scale_type.dart';
import '../../common/extensions/string_extensions.dart';
import '../../generated/i18n.dart';
import '../../models/dtos/responses/app_settings_response_dto.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';
import '../server_ws/server_ws_bloc.dart';

part 'settings_bloc.freezed.dart';
part 'settings_event.dart';
part 'settings_state.dart';

class SettingsBloc extends Bloc<SettingsEvent, SettingsState> {
  final LoggingService _logger;
  final SettingsService _settings;

  final ServerWsBloc _serverWsBloc;

  SettingsBloc(this._logger, this._settings, this._serverWsBloc) {
    _serverWsBloc.settingsChanged.stream.listen((settings) {
      add(SettingsEvent.connected(settings: settings));
    });

    _serverWsBloc.disconnected.stream.listen((_) {
      add(SettingsEvent.disconnected());
    });
  }

  @override
  SettingsState get initialState => SettingsState.loading();

  SettingsLoadedState get currentState => state as SettingsLoadedState;

  @override
  Stream<SettingsState> mapEventToState(
    SettingsEvent event,
  ) async* {
    final s = await event.map(
      load: (event) async {
        await _settings.init();
        final packageInfo = await PackageInfo.fromPlatform();
        final settings = _settings.appSettings;
        return SettingsState.loaded(
          appTheme: settings.appTheme,
          useDarkAmoled: settings.useDarkAmoled,
          accentColor: settings.accentColor,
          appLanguage: settings.appLanguage,
          castItUrl: settings.castItUrl,
          isCastItUrlValid: true,
          videoScale: VideoScaleType.original,
          enableHwAccel: false,
          forceAudioTranscode: false,
          forceVideoTranscode: false,
          isConected: false,
          playFromTheStart: false,
          playNextFileAutomatically: false,
          appName: packageInfo.appName,
          appVersion: packageInfo.version,
        );
      },
      connected: (event) async {
        return currentState.copyWith(
          isConected: true,
          videoScale: event.settings.videoScaleType,
          enableHwAccel: event.settings.enableHwAccel,
          forceAudioTranscode: event.settings.forceAudioTranscode,
          forceVideoTranscode: event.settings.forceVideoTranscode,
          playFromTheStart: event.settings.playFromTheStart,
          playNextFileAutomatically: event.settings.playNextFileAutomatically,
        );
      },
      disconnected: (event) async => currentState.copyWith(isConected: false),
      themeChanged: (event) async {
        _settings.appTheme = event.theme;
        return currentState.copyWith(appTheme: event.theme);
      },
      accentColorChanged: (event) async {
        _settings.accentColor = event.accentColor;
        return currentState.copyWith(accentColor: event.accentColor);
      },
      languageChanged: (event) async {
        _settings.language = event.lang;
        final locale = I18n.delegate.supportedLocales[event.lang.index];
        I18n.onLocaleChanged(locale);
        return currentState.copyWith(appLanguage: event.lang);
      },
      castItUrlChanged: (event) async {
        final isValid = _isCastItUrlValid(event.castItUrl);
        if (isValid) _settings.castItUrl = event.castItUrl;
        return currentState.copyWith(isCastItUrlValid: isValid, castItUrl: event.castItUrl);
      },
    );

    yield s;
  }

  bool _isCastItUrlValid(String url) {
    return !url.isNullEmptyOrWhitespace && Uri.parse(url).isAbsolute;
  }
}
