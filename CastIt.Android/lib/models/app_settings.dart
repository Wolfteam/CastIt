import 'package:flutter/foundation.dart';
import 'package:flutter/widgets.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../common/enums/app_accent_color_type.dart';
import '../common/enums/app_language_type.dart';
import '../common/enums/app_theme_type.dart';

part 'app_settings.freezed.dart';
part 'app_settings.g.dart';

@freezed
abstract class AppSettings implements _$AppSettings {
  // final AppThemeType appTheme;
  // final bool useDarkAmoled;
  // final AppAccentColorType accentColor;
  // final AppLanguageType appLanguage;
  // final String castItUrl;

  // @override
  // List<Object> get props => [
  //       appTheme,
  //       useDarkAmoled,
  //       accentColor,
  //       appLanguage,
  //       castItUrl,
  //     ];
  factory AppSettings({
    @required AppThemeType appTheme,
    @required bool useDarkAmoled,
    @required AppAccentColorType accentColor,
    @required AppLanguageType appLanguage,
    @required String castItUrl,
  }) = _AppSettings;
  const AppSettings._();

  factory AppSettings.fromJson(Map<String, dynamic> json) =>
      _$AppSettingsFromJson(json);
}

// abstract class AppSettings extends Equatable {
//   final AppThemeType appTheme;
//   final bool useDarkAmoled;
//   final AppAccentColorType accentColor;
//   final AppLanguageType appLanguage;
//   final String castItUrl;

//   @override
//   List<Object> get props => [
//         appTheme,
//         useDarkAmoled,
//         accentColor,
//         appLanguage,
//         castItUrl,
//       ];

//   const AppSettings({
//     @required this.appTheme,
//     @required this.useDarkAmoled,
//     @required this.accentColor,
//     @required this.appLanguage,
//     @required this.castItUrl,
//   });

//   factory AppSettings.fromJson(Map<String, dynamic> json) =>
//       _$AppSettingsFromJson(json);
//   Map<String, dynamic> toJson() => _$AppSettingsToJson(this);
// }
