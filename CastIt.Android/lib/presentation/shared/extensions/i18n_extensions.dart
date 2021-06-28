import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';

extension I18nExtensions on S {
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

  String translateAppMsgType(AppMessageType type) {
    //TODO: SWITCH THE I18N PROVIDER
    return 'N/A';

    switch (type) {
      case AppMessageType.unknownErrorOccurred:
        // TODO: Handle this case.
        break;
      case AppMessageType.invalidRequest:
        // TODO: Handle this case.
        break;
      case AppMessageType.notFound:
        // TODO: Handle this case.
        break;
      case AppMessageType.playListNotFound:
        // TODO: Handle this case.
        break;
      case AppMessageType.unknownErrorLoadingFile:
        // TODO: Handle this case.
        break;
      case AppMessageType.fileNotFound:
        // TODO: Handle this case.
        break;
      case AppMessageType.fileIsAlreadyBeingPlayed:
        // TODO: Handle this case.
        break;
      case AppMessageType.fileNotSupported:
        // TODO: Handle this case.
        break;
      case AppMessageType.filesAreNotValid:
        // TODO: Handle this case.
        break;
      case AppMessageType.noFilesToBeAdded:
        // TODO: Handle this case.
        break;
      case AppMessageType.urlNotSupported:
        // TODO: Handle this case.
        break;
      case AppMessageType.urlCouldntBeParsed:
        // TODO: Handle this case.
        break;
      case AppMessageType.oneOrMoreFilesAreNotReadyYet:
        // TODO: Handle this case.
        break;
      case AppMessageType.noDevicesFound:
        // TODO: Handle this case.
        break;
      case AppMessageType.noInternetConnection:
        // TODO: Handle this case.
        break;
      case AppMessageType.connectionToDeviceIsStillInProgress:
        // TODO: Handle this case.
        break;
      case AppMessageType.ffmpegError:
        // TODO: Handle this case.
        break;
      case AppMessageType.serverIsClosing:
        // TODO: Handle this case.
        break;
    }
  }
}
