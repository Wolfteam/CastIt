import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';

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

  bool get isFirstInstall;

  set isFirstInstall(bool itIs);

  Future init();
}
