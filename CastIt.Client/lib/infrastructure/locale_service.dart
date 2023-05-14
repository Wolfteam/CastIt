import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/domain/services/locale_service.dart';
import 'package:castit/domain/services/settings_service.dart';

const _languagesMap = {
  AppLanguageType.english: LanguageModel('en', 'US'),
  AppLanguageType.spanish: LanguageModel('es', 'ES'),
};

class LocaleServiceImpl implements LocaleService {
  final SettingsService _settingsService;

  LocaleServiceImpl(this._settingsService);

  @override
  LanguageModel getCurrentLocale() {
    return getLocale(_settingsService.language);
  }

  @override
  LanguageModel getLocale(AppLanguageType language) {
    if (!_languagesMap.entries.any((kvp) => kvp.key == language)) {
      throw Exception('The language = $language is not a valid value');
    }

    return _languagesMap.entries.firstWhere((kvp) => kvp.key == language).value;
  }
}
