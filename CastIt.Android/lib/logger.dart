import 'package:flutter/material.dart';
import 'package:log_4_dart_2/log_4_dart_2.dart';

import 'common/utils/app_path_utils.dart';

Future<void> setupLogging() async {
  WidgetsFlutterBinding.ensureInitialized();
  final dirPath = await AppPathUtils.logsPath;
  final config = {
    'appenders': [
      {
        'type': 'CONSOLE',
        'dateFormat': 'yyyy-MM-dd HH:mm:ss',
        'format': '%d %i %t %l %m',
        'level': 'INFO',
      },
      {
        'type': 'FILE',
        'dateFormat': 'dd-MM-yyyy HH:mm:ss',
        'format': '%d [%l] - [%t]: %m',
        'level': 'ALL',
        'filePattern': 'castit_log',
        'fileExtension': 'txt',
        'path': '$dirPath/',
        'rotationCycle': 'DAY'
      },
    ]
  };
  Logger().init(config);
}
