import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/enums/subtitle_bg_color_type.dart';
import 'package:castit/domain/enums/text_track_font_generic_family_type.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/castit_hub_client_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info_plus/package_info_plus.dart';

part 'settings_bloc.freezed.dart';
part 'settings_event.dart';
part 'settings_state.dart';

class SettingsBloc extends Bloc<SettingsEvent, SettingsState> {
  final SettingsService _settings;
  final MainBloc _mainBloc;
  final CastItHubClientService _castItHub;

  SettingsState get initialState => SettingsState.loading();

  SettingsBloc(this._settings, this._mainBloc, this._castItHub) : super(SettingsState.loading());

  _LoadedState get currentState => state as _LoadedState;

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
          fFmpegExePath: '',
          fFprobeExePath: '',
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
          fFmpegExePath: event.settings.fFmpegExePath,
          fFprobeExePath: event.settings.fFprobeExePath,
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
        if (event.lang == _settings.language) {
          return currentState;
        }
        _settings.language = event.lang;
        _mainBloc.add(MainEvent.languageChanged());
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
    _castItHub.connected.stream.listen((event) {
      add(SettingsEvent.load());
    });

    _castItHub.settingsChanged.stream.listen((settings) {
      add(SettingsEvent.connected(settings: settings));
    });

    _castItHub.disconnected.stream.listen((_) {
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
