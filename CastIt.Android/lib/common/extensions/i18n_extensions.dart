import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../generated/i18n.dart';
import '../enums/video_scale_type.dart';

extension I18nExtensions on I18n {
  String translateAppThemeType(AppThemeType theme) {
    switch (theme) {
      case AppThemeType.dark:
        return dark;
      case AppThemeType.light:
        return light;
      default:
        throw Exception('The provided app theme = $theme is not valid');
    }
  }

  String translateAppLanguageType(AppLanguageType lang) {
    switch (lang) {
      case AppLanguageType.english:
        return english;
      case AppLanguageType.spanish:
        return spanish;
      default:
        throw Exception('The provided app lang = $lang is not valid');
    }
  }

  String translateVideoScaleType(VideoScaleType scale) {
    switch (scale) {
      case VideoScaleType.fullHd:
        return fullHd;
      case VideoScaleType.hd:
        return hd;
      case VideoScaleType.original:
        return original;
      default:
        throw Exception('The provided video scale = $scale is not valid');
    }
  }
}
