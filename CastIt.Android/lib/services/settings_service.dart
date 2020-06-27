import 'package:shared_preferences/shared_preferences.dart';

import '../common/enums/app_accent_color_type.dart';
import '../common/enums/app_language_type.dart';
import '../common/enums/app_theme_type.dart';
import '../models/app_settings.dart';
import 'logging_service.dart';

abstract class SettingsService {
  AppSettings get appSettings;

  AppThemeType get appTheme;
  set appTheme(AppThemeType theme);

  AppAccentColorType get accentColor;
  set accentColor(AppAccentColorType accentColor);

  AppLanguageType get language;
  set language(AppLanguageType lang);

  String get castItUrl;
  set castItUrl(String url);

  Future init();
}

class SettingsServiceImpl extends SettingsService {
  final _appThemeKey = 'AppTheme';
  final _accentColorKey = 'AccentColor';
  final _appLanguageKey = 'AppLanguage';
  final _castItUrlKey = 'CastItUrl';

  bool _initialized = false;

  SharedPreferences _prefs;
  final LoggingService _logger;

  @override
  AppThemeType get appTheme =>
      AppThemeType.values[(_prefs.getInt(_appThemeKey))];
  @override
  set appTheme(AppThemeType theme) => _prefs.setInt(_appThemeKey, theme.index);

  @override
  AppAccentColorType get accentColor =>
      AppAccentColorType.values[_prefs.getInt(_accentColorKey)];
  @override
  set accentColor(AppAccentColorType accentColor) =>
      _prefs.setInt(_accentColorKey, accentColor.index);

  @override
  AppLanguageType get language =>
      AppLanguageType.values[_prefs.getInt(_appLanguageKey)];
  @override
  set language(AppLanguageType lang) =>
      _prefs.setInt(_appLanguageKey, lang.index);

  @override
  String get castItUrl => _prefs.getString(_castItUrlKey);
  @override
  set castItUrl(String url) => _prefs.setString(_castItUrlKey, url);

  @override
  AppSettings get appSettings => AppSettings(
        appTheme: appTheme,
        useDarkAmoled: false,
        accentColor: accentColor,
        appLanguage: language,
        castItUrl: castItUrl,
      );

  SettingsServiceImpl(this._logger);

  @override
  Future init() async {
    if (_initialized) {
      _logger.warning(runtimeType, 'Settings are already initialized!');
      return;
    }

    _logger.info(runtimeType, 'Getting shared prefs instance...');

    _prefs = await SharedPreferences.getInstance();

    if (_prefs.get(_appThemeKey) == null) {
      _logger.info(runtimeType, 'Setting default dark theme');
      _prefs.setInt(_appThemeKey, AppThemeType.dark.index);
    }

    if (_prefs.get(_accentColorKey) == null) {
      _logger.info(runtimeType, 'Setting default blue accent color');
      _prefs.setInt(_accentColorKey, AppAccentColorType.red.index);
    }

    if (_prefs.get(_appLanguageKey) == null) {
      _logger.info(runtimeType, 'Setting english as the default lang');
      _prefs.setInt(_appLanguageKey, AppLanguageType.english.index);
    }

    if (_prefs.get(_castItUrlKey) == null) {
      _logger.info(runtimeType, 'Setting url to the default one');
      _prefs.setString(_castItUrlKey, 'ws://192.168.1.101:9696/socket');
    }

    _initialized = true;
    _logger.info(runtimeType, 'Settings were initialized successfully');
  }
}
