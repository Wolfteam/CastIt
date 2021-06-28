import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/common/enums/subtitle_bg_color_type.dart';
import 'package:castit/common/enums/subtitle_fg_color_type.dart';
import 'package:castit/common/enums/subtitle_font_scale_type.dart';
import 'package:castit/common/enums/text_track_font_generic_family_type.dart';
import 'package:castit/common/enums/text_track_font_style_type.dart';
import 'package:castit/models/server_app_settings.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info_plus/package_info_plus.dart';

import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/enums/video_scale_type.dart';
import '../../common/extensions/string_extensions.dart';
import '../../generated/i18n.dart';
import '../../services/settings_service.dart';
import '../server_ws/server_ws_bloc.dart';

part 'settings_bloc.freezed.dart';
part 'settings_event.dart';
part 'settings_state.dart';

class SettingsBloc extends Bloc<SettingsEvent, SettingsState> {
  final SettingsService _settings;

  final ServerWsBloc _serverWsBloc;

  SettingsState get initialState => SettingsState.loading();

  SettingsBloc(this._settings, this._serverWsBloc) : super(SettingsState.loading());

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
          isConnected: false,
          playFromTheStart: false,
          playNextFileAutomatically: false,
          appName: packageInfo.appName,
          appVersion: packageInfo.version,
          currentSubtitleBgColor: SubtitleBgColorType.transparent,
          currentSubtitleFgColor: SubtitleFgColorType.white,
          currentSubtitleFontScale: SubtitleFontScaleType.hundredAndFifty,
          currentSubtitleFontFamily: TextTrackFontGenericFamilyType.casual,
          currentSubtitleFontStyle: TextTrackFontStyleType.normal,
          loadFirstSubtitleFoundAutomatically: true,
          subtitleDelayInSeconds: 0,
        );
      },
      connected: (event) async {
        return currentState.copyWith(
          isConnected: true,
          videoScale: event.settings.videoScaleType,
          enableHwAccel: event.settings.enableHardwareAcceleration,
          forceAudioTranscode: event.settings.forceAudioTranscode,
          forceVideoTranscode: event.settings.forceVideoTranscode,
          playFromTheStart: event.settings.startFilesFromTheStart,
          playNextFileAutomatically: event.settings.playNextFileAutomatically,
          //subs
          currentSubtitleBgColor: event.settings.currentSubtitleBgColorType,
          currentSubtitleFgColor: event.settings.currentSubtitleFgColorType,
          currentSubtitleFontScale: event.settings.currentSubtitleFontScaleType,
          currentSubtitleFontFamily: event.settings.currentSubtitleFontFamilyType,
          currentSubtitleFontStyle: event.settings.currentSubtitleFontStyleType,
          loadFirstSubtitleFoundAutomatically: event.settings.loadFirstSubtitleFoundAutomatically,
          subtitleDelayInSeconds: event.settings.subtitleDelayInSeconds,
        );
      },
      disconnected: (event) async => currentState.copyWith(isConnected: false),
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

  void listenHubEvents() {
    _serverWsBloc.connected.stream.listen((event) {
      add(SettingsEvent.load());
    });

    _serverWsBloc.settingsChanged.stream.listen((settings) {
      add(SettingsEvent.connected(settings: settings!));
    });

    _serverWsBloc.disconnected.stream.listen((_) {
      add(SettingsEvent.disconnected());
    });
  }

  bool _isCastItUrlValid(String url) {
    try {
      return !url.isNullEmptyOrWhitespace && url.startsWith('http://') && Uri.parse(url).isAbsolute;
    } catch (e) {
      return false;
    }
  }
}
