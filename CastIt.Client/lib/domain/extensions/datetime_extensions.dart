import 'package:intl/intl.dart';

extension DatetimeExtensions on DateTime? {
  String? formatLastPlayedDate() {
    if (this == null) {
      return null;
    }
    final DateFormat formatter = DateFormat('yyyy-MM-dd');
    return formatter.format(this!);
  }
}
