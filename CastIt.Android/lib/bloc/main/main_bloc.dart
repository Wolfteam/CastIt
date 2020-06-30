import 'dart:async';
import 'dart:convert';

import 'package:bloc/bloc.dart';
import 'package:castit/models/dtos/responses/app_settings_response_dto.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:package_info/package_info.dart';
import 'package:web_socket_channel/io.dart';
import 'package:web_socket_channel/status.dart' as status_codes;

import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/extensions/app_theme_type_extensions.dart';
import '../../generated/i18n.dart';
import '../../models/dtos/responses/file_loaded_response_dto.dart';
import '../../models/dtos/socket_response_dto.dart';
import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'main_bloc.freezed.dart';
part 'main_event.dart';
part 'main_state.dart';

class MainBloc extends Bloc<MainEvent, MainState> {
  static const String _clientConnectedMsgType = 'CLIENT_CONNECTED';
  static const String _fileLoadingMsgType = 'FILE_LOADING';
  static const String _fileLoadedMsgType = 'FILE_LOADED';
  static const String _fileLoadingErrorMsgType = 'ERROR_ON_FILE_LOADING';
  static const String _filePositionChangedMsgType = 'FILE_POSITION_CHANGED';
  static const String _fileTimeChangedMsgType = 'FILE_TIME_CHANGED';
  static const String _filePausedMsgType = 'FILE_PAUSED';
  static const String _fileEndReachedMsgType = 'FILE_END_REACHED';
  static const String _disconectedMsgType = 'DISCONNECTED';
  static const String _volumeChangedMsgType = 'VOLUME_LEVEL_CHANGED';
  static const String _appClosingMsgType = 'APP_CLOSING';
  static const String _appSettingsChangedMsgType = 'APP_SETTINGS_CHANGED';

  final LoggingService _logger;
  final SettingsService _settings;

  //TODO:We may need to use rxdart BehaviourSubject
  final StreamController<void> connected = StreamController.broadcast();
  final StreamController<void> fileLoading = StreamController.broadcast();
  final StreamController<FileLoadedResponseDto> fileLoaded = StreamController.broadcast();
  final StreamController<String> fileLoadingError = StreamController.broadcast();
  final StreamController<double> fileTimeChanged = StreamController.broadcast();
  final StreamController<void> filePaused = StreamController.broadcast();
  final StreamController<void> fileEndReached = StreamController.broadcast();
  final StreamController<void> disconnected = StreamController.broadcast();
  final StreamController<void> appClosing = StreamController.broadcast();
  final StreamController<AppSettingsResponseDto> settingsChanged = StreamController.broadcast();

  IOWebSocketChannel _channel;

  @override
  MainState get initialState => MainState.loading();

  MainLoadedState get currentState => state as MainLoadedState;

  MainBloc(this._logger, this._settings);

  @override
  Stream<MainState> mapEventToState(
    MainEvent event,
  ) async* {
    final s = event.when(
      disconnectFromWs: () {
        _disconnectFromWs();
        return null;
      },
      connectToWs: () {
        _connectToWs();
        return null;
      },
      init: _init,
      themeChanged: (theme) => _loadThemeData(
        currentState.appTitle,
        theme,
        _settings.accentColor,
        _settings.language,
      ),
      accentColorChanged: (accentColor) => _loadThemeData(
        currentState.appTitle,
        _settings.appTheme,
        accentColor,
        _settings.language,
      ),
    );

    if (s != null) yield* s;
  }

  //TODO: CLOSE THIS SUBSCRIPTION AND ALL THE STREAMS
  @override
  Future<void> close() {
    _disconnectFromWs();
    return super.close();
  }

  Stream<MainState> _init() async* {
    await _settings.init();
    final packageInfo = await PackageInfo.fromPlatform();
    final appSettings = _settings.appSettings;
    yield* _loadThemeData(packageInfo.appName, appSettings.appTheme, appSettings.accentColor, appSettings.appLanguage);
  }

  Stream<MainState> _loadThemeData(
    String appTitle,
    AppThemeType theme,
    AppAccentColorType accentColor,
    AppLanguageType language, {
    bool isInitialized = true,
  }) async* {
    final themeData = accentColor.getThemeData(theme);
    _setLocale(language);

    yield MainState.loaded(
      appTitle: appTitle,
      initialized: isInitialized,
      theme: themeData,
    );
  }

  void _setLocale(AppLanguageType language) {
    final locale = I18n.delegate.supportedLocales[language.index];
    I18n.onLocaleChanged(locale);
  }

  void _connectToWs() {
    try {
      final url = _settings.appSettings.castItUrl;
      _disconnectFromWs();
      _channel = IOWebSocketChannel.connect(url);

      _channel.stream.listen((event) {
        final jsonMap = json.decode(event as String) as Map<String, dynamic>;
        _handleSocketMsg(jsonMap);
      }, onError: (e, StackTrace s) {
        _logger.error(runtimeType, '_connectToWs: Error while listening in channel', e, s);
      }, onDone: () {
        _logger.info(runtimeType, '_connectToWs: disconnected from ws');
      });
    } catch (e, s) {
      _logger.error(runtimeType, '_connectToWs: Unknown error', e, s);
    }
  }

  void _disconnectFromWs() {
    if (_channel != null) {
      _channel.sink.close(status_codes.goingAway);
    }
  }

  void _handleSocketMsg(Map<String, dynamic> json) {
    if (!json.containsKey('MessageType')) return;
    final msgType = json['MessageType'] as String;
    final response = SocketResponseDto.fromJson(json);
    switch (msgType) {
      case _clientConnectedMsgType:
        connected.add(null);
        break;
      case _fileLoadingMsgType:
        fileLoading.add(null);
        break;
      case _fileLoadedMsgType:
        final file = response.result as FileLoadedResponseDto;
        fileLoaded.add(file);
        break;
      case _fileLoadingErrorMsgType:
        fileLoadingError.add('Error loading file');
        break;
      case _filePositionChangedMsgType:
        // final filePositionChanged = SocketResponseDto.fromJson(json);
        break;
      case _fileTimeChangedMsgType:
        fileTimeChanged.add(response.result as double);
        break;
      case _volumeChangedMsgType:
        // final volumeChanged = SocketResponseDto.fromJson(json);
        break;
      case _fileEndReachedMsgType:
        fileEndReached.add(null);
        break;
      case _filePausedMsgType:
        filePaused.add(null);
        break;
      case _disconectedMsgType:
        disconnected.add(null);
        break;
      case _appClosingMsgType:
        appClosing.add(null);
        break;
      case _appSettingsChangedMsgType:
        final settings = response.result as AppSettingsResponseDto;
        settingsChanged.add(settings);
        break;
      default:
        _logger.warning(
          runtimeType,
          '_handleSocketMsg: Msg = $msgType is not being handled',
        );
        break;
    }
  }
}
