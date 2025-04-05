import 'package:bloc/bloc.dart';
import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
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

  SettingsState get initialState => const SettingsState.loading();

  SettingsStateLoadedState get currentState => state as SettingsStateLoadedState;

  SettingsBloc(this._settings, this._mainBloc, this._castItHub) : super(const SettingsState.loading()) {
    on<SettingsEventLoad>((event, emit) async {
      if (state is SettingsStateLoadedState) {
        return;
      }
      await _settings.init();
      final packageInfo = await PackageInfo.fromPlatform();
      final settings = _settings.appSettings;
      final updatedState = SettingsState.loaded(
        appTheme: settings.appTheme,
        useDarkAmoled: settings.useDarkAmoled,
        accentColor: settings.accentColor,
        appLanguage: settings.appLanguage,
        castItUrl: settings.castItUrl,
        isCastItUrlValid: true,
        fFmpegExePath: '',
        fFprobeExePath: '',
        videoScale: VideoScaleType.original,
        webVideoQuality: WebVideoQualityType.low,
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

      emit(updatedState);
    });

    on<SettingsEventConnected>((event, emit) {
      final updatedState = currentState.copyWith(
        isConnected: true,
        fFmpegExePath: event.settings.fFmpegExePath,
        fFprobeExePath: event.settings.fFprobeExePath,
        videoScale: event.settings.videoScaleType,
        enableHwAccel: event.settings.enableHardwareAcceleration,
        forceAudioTranscode: event.settings.forceAudioTranscode,
        forceVideoTranscode: event.settings.forceVideoTranscode,
        playFromTheStart: event.settings.startFilesFromTheStart,
        playNextFileAutomatically: event.settings.playNextFileAutomatically,
        webVideoQuality: event.settings.webVideoQualityType,
        //subs
        currentSubtitleBgColor: event.settings.currentSubtitleBgColorType,
        currentSubtitleFgColor: event.settings.currentSubtitleFgColorType,
        currentSubtitleFontScale: event.settings.currentSubtitleFontScaleType,
        currentSubtitleFontFamily: event.settings.currentSubtitleFontFamilyType,
        currentSubtitleFontStyle: event.settings.currentSubtitleFontStyleType,
        loadFirstSubtitleFoundAutomatically: event.settings.loadFirstSubtitleFoundAutomatically,
        subtitleDelayInSeconds: event.settings.subtitleDelayInSeconds,
      );

      emit(updatedState);
    });

    on<SettingsEventDisconnected>((event, emit) => emit(currentState.copyWith(isConnected: false)));

    on<SettingsEventThemeChanged>((event, emit) {
      _settings.appTheme = event.theme;
      final updatedState = currentState.copyWith(appTheme: event.theme);
      emit(updatedState);
    });

    on<SettingsEventAccentColorChanged>((event, emit) {
      _settings.accentColor = event.accentColor;
      final updatedState = currentState.copyWith(accentColor: event.accentColor);
      emit(updatedState);
    });

    on<SettingsEventLanguageChanged>((event, emit) {
      if (event.lang == _settings.language) {
        return;
      }
      _settings.language = event.lang;
      _mainBloc.add(const MainEvent.languageChanged());
      final updatedState = currentState.copyWith(appLanguage: event.lang);
      emit(updatedState);
    });

    on<SettingsEventCastItUrlChanged>((event, emit) {
      final isValid = _isCastItUrlValid(event.castItUrl);
      if (isValid) {
        _settings.castItUrl = event.castItUrl;
      }
      final updatedState = currentState.copyWith(isCastItUrlValid: isValid, castItUrl: event.castItUrl);
      emit(updatedState);
    });
  }

  void listenHubEvents() {
    _castItHub.connected.stream.listen((event) {
      add(const SettingsEvent.load());
    });

    _castItHub.settingsChanged.stream.listen((settings) {
      add(SettingsEvent.connected(settings: settings));
    });

    _castItHub.disconnected.stream.listen((_) {
      add(const SettingsEvent.disconnected());
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
