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
    switch (type) {
      case AppMessageType.unknownErrorOccurred:
        return unknownErrorOccurred;
      case AppMessageType.invalidRequest:
        return invalidRequest;
      case AppMessageType.notFound:
        return notFound;
      case AppMessageType.playListNotFound:
        return playListNotFound;
      case AppMessageType.unknownErrorLoadingFile:
        return unknownErrorLoadingFile;
      case AppMessageType.fileNotFound:
        return fileNotFound;
      case AppMessageType.fileIsAlreadyBeingPlayed:
        return fileIsAlreadyBeingPlayed;
      case AppMessageType.fileNotSupported:
        return fileNotSupported;
      case AppMessageType.filesAreNotValid:
        return filesAreNotValid;
      case AppMessageType.noFilesToBeAdded:
        return noFilesToBeAdded;
      case AppMessageType.urlNotSupported:
        return urlNotSupported;
      case AppMessageType.urlCouldntBeParsed:
        return urlCouldntBeParsed;
      case AppMessageType.oneOrMoreFilesAreNotReadyYet:
        return oneOrMoreFilesAreNotReadyYet;
      case AppMessageType.noDevicesFound:
        return noDevicesFound;
      case AppMessageType.noInternetConnection:
        return noInternetConnection;
      case AppMessageType.connectionToDeviceIsStillInProgress:
        return connectionToDeviceIsStillInProgress;
      case AppMessageType.ffmpegError:
        return ffmpegError;
      case AppMessageType.serverIsClosing:
        return serverIsClosing;
      case AppMessageType.ffmpegExecutableNotFound:
        return ffmpegExecutableNotFound;
    }
  }
}
