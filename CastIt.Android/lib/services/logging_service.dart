import 'package:flutter/foundation.dart';
import 'package:logger/logger.dart';
import 'package:sprintf/sprintf.dart';

import '../common/extensions/string_extensions.dart';
import '../telemetry.dart';

abstract class LoggingService {
  void info(Type type, String msg, [List<Object> args]);

  void warning(Type type, String msg, [dynamic ex, StackTrace trace]);

  void error(Type type, String msg, [dynamic ex, StackTrace trace]);
}

class LoggingServiceImpl implements LoggingService {
  final _logger = Logger();

  LoggingServiceImpl();

  @override
  void info(Type type, String msg, [List<Object> args]) {
    assert(type != null && !msg.isNullEmptyOrWhitespace);

    if (args != null && args.isNotEmpty) {
      _logger.i(type.toString(), sprintf(msg, args));
    } else {
      _logger.i(type.toString(), msg);
    }
  }

  @override
  void warning(Type type, String msg, [dynamic ex, StackTrace trace]) {
    assert(type != null && !msg.isNullEmptyOrWhitespace);
    final tag = type.toString();
    _logger.w('$tag - ${_formatEx(msg, ex)}', ex, trace);
    if (kReleaseMode) {
      _trackWarning(tag, msg, ex, trace);
    }
  }

  @override
  void error(Type type, String msg, [dynamic ex, StackTrace trace]) {
    assert(type != null && !msg.isNullEmptyOrWhitespace);
    final tag = type.toString();
    _logger.e('$tag - ${_formatEx(msg, ex)}', ex, trace);

    if (kReleaseMode) {
      _trackError(tag, msg, ex, trace);
    }
  }

  String _formatEx(String msg, dynamic ex) {
    if (ex != null) {
      return '$msg \n $ex';
    }
    return '$msg \n No exception available';
  }

  void _trackError(String tag, String msg, [dynamic ex, StackTrace trace]) {
    final map = {
      'tag': tag,
      'msg': _formatEx(msg, ex),
      'trace': trace?.toString() ?? 'No trace available',
    };
    trackEventAsync('Error - ${DateTime.now()}', map);
  }

  void _trackWarning(String tag, String msg, [dynamic ex, StackTrace trace]) {
    final map = {
      'tag': tag,
      'msg': _formatEx(msg, ex),
      'trace': trace?.toString() ?? 'No trace available',
    };
    trackEventAsync('Warning - ${DateTime.now()}', map);
  }
}
