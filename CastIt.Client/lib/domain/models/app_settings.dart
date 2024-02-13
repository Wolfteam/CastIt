import 'package:castit/domain/enums/enums.dart';
import 'package:flutter/foundation.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'app_settings.freezed.dart';
part 'app_settings.g.dart';

@freezed
class AppSettings with _$AppSettings {
  factory AppSettings({
    required AppThemeType appTheme,
    required bool useDarkAmoled,
    required AppAccentColorType accentColor,
    required AppLanguageType appLanguage,
    required String castItUrl,
  }) = _AppSettings;

  const AppSettings._();

  factory AppSettings.fromJson(Map<String, dynamic> json) => _$AppSettingsFromJson(json);
}
