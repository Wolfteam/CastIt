import 'dart:io';

import 'package:path/path.dart';
import 'package:path_provider/path_provider.dart';

class AppPathUtils {
  //internal memory/android/data/com.miraisoft.my_expenses/files/logs
  static Future<String> get logsPath async {
    final dir = await getExternalStorageDirectory();
    final dirPath = '${dir.path}/Logs';
    await _generateDirectoryIfItDoesntExist(dirPath);
    return dirPath;
  }

  static Future<void> _generateDirectoryIfItDoesntExist(String path) async {
    final dirExists = await Directory(path).exists();
    if (!dirExists) {
      await Directory(path).create(recursive: true);
    }
  }
}
