import 'package:log_4_dart_2/log_4_dart_2.dart';
import 'package:sprintf/sprintf.dart';

abstract class LoggingService {
  void info(Type type, String msg, [List<Object> args]);

  void warning(Type type, String msg, [dynamic ex, StackTrace trace]);

  void error(Type type, String msg, [dynamic ex, StackTrace trace]);
}

class LoggingServiceImpl implements LoggingService {
  final Logger _logger;

  LoggingServiceImpl(this._logger);

  @override
  void info(Type type, String msg, [List<Object> args]) {
    if (args != null && args.isNotEmpty) {
      _logger.info(type.toString(), sprintf(msg, args));
    } else {
      _logger.info(type.toString(), msg);
    }
  }

  @override
  void warning(Type type, String msg, [dynamic ex, StackTrace trace]) {
    if (ex != null) {
      _logger.warning(type.toString(), _formatEx(msg, ex), ex, trace);
    } else {
      _logger.warning(type.toString(), msg, ex, trace);
    }
  }

  @override
  void error(Type type, String msg, [dynamic ex, StackTrace trace]) {
    if (ex != null) {
      _logger.error(type.toString(), _formatEx(msg, ex), ex, trace);
    } else {
      _logger.error(type.toString(), _formatEx(msg, ex), ex, trace);
    }
  }

  String _formatEx(String msg, dynamic ex) {
    return '$msg \n $ex';
  }
}
