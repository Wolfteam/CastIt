import 'package:castit/domain/app_constants.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/logging_service.dart';
import 'package:castit/domain/services/settings_service.dart';
import 'package:network_info_plus/network_info_plus.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SettingsServiceImpl extends SettingsService {
  final _appThemeKey = 'AppTheme';
  final _accentColorKey = 'AccentColor';
  final _appLanguageKey = 'AppLanguage';
  final _castItUrlKey = 'CastItUrl';
  final _firstInstallKey = 'FirstInstall';

  bool _initialized = false;

  late SharedPreferences _prefs;
  final LoggingService _logger;

  @override
  AppThemeType get appTheme => AppThemeType.values[_prefs.getInt(_appThemeKey)!];

  @override
  set appTheme(AppThemeType theme) => _prefs.setInt(_appThemeKey, theme.index);

  @override
  AppAccentColorType get accentColor => AppAccentColorType.values[_prefs.getInt(_accentColorKey)!];

  @override
  set accentColor(AppAccentColorType accentColor) => _prefs.setInt(_accentColorKey, accentColor.index);

  @override
  AppLanguageType get language => AppLanguageType.values[_prefs.getInt(_appLanguageKey)!];

  @override
  set language(AppLanguageType lang) => _prefs.setInt(_appLanguageKey, lang.index);

  @override
  String get castItUrl => _prefs.getString(_castItUrlKey)!;

  @override
  set castItUrl(String url) => _prefs.setString(_castItUrlKey, url);

  @override
  bool get isFirstInstall => _prefs.getBool(_firstInstallKey)!;

  @override
  set isFirstInstall(bool itIs) => _prefs.setBool(_firstInstallKey, itIs);

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
      _logger.info(runtimeType, 'Settings are already initialized!');
      return;
    }

    _logger.info(runtimeType, 'Getting shared prefs instance...');

    _prefs = await SharedPreferences.getInstance();

    if (_prefs.get(_firstInstallKey) == null) {
      _logger.info(runtimeType, 'This is the first install of the app');
      isFirstInstall = true;
    }

    if (_prefs.get(_appThemeKey) == null) {
      _logger.info(runtimeType, 'Setting default dark theme');
      appTheme = AppThemeType.dark;
    }

    if (_prefs.get(_accentColorKey) == null) {
      _logger.info(runtimeType, 'Setting default blue accent color');
      accentColor = AppAccentColorType.red;
    }

    if (_prefs.get(_appLanguageKey) == null) {
      _logger.info(runtimeType, 'Setting english as the default lang');
      language = AppLanguageType.english;
    }

    if (_prefs.get(_castItUrlKey) == null) {
      final url = await _getWifiIP();
      _logger.info(runtimeType, 'Setting url to = $url');
      castItUrl = url;
    }
    _initialized = true;
    _logger.info(runtimeType, 'Settings were initialized successfully');
  }

  Future<String> _getWifiIP() async {
    try {
      final ip = await NetworkInfo().getWifiIP();

      if (!ip.isNullEmptyOrWhitespace) {
        return 'http://$ip:9696';
      }
      return AppConstants.baseCastItUrl;
    } catch (e, s) {
      _logger.error(runtimeType, '_getWifiIP: Unknown error , falling back to ${AppConstants.baseCastItUrl}', e, s);
      return AppConstants.baseCastItUrl;
    }
  }
}
