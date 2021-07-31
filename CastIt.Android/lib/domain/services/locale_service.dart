import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';

abstract class LocaleService {
  LanguageModel getCurrentLocale();

  LanguageModel getLocale(AppLanguageType language);
}
