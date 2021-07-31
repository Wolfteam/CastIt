enum AppMessageType {
  unknownErrorOccurred,
  invalidRequest,
  notFound,
  playListNotFound,
  unknownErrorLoadingFile,
  fileNotFound,
  fileIsAlreadyBeingPlayed,
  fileNotSupported,
  filesAreNotValid,
  noFilesToBeAdded,
  urlNotSupported,
  urlCouldntBeParsed,
  oneOrMoreFilesAreNotReadyYet,
  noDevicesFound,
  noInternetConnection,
  connectionToDeviceIsStillInProgress,
  ffmpegError,
  serverIsClosing,
  ffmpegExecutableNotFound,
}

AppMessageType getAppMessageType(int value) {
  switch (value) {
    case 1:
      return AppMessageType.unknownErrorOccurred;
    case 2:
      return AppMessageType.invalidRequest;
    case 3:
      return AppMessageType.notFound;
    case 100:
      return AppMessageType.playListNotFound;
    case 200:
      return AppMessageType.unknownErrorLoadingFile;
    case 201:
      return AppMessageType.fileNotFound;
    case 202:
      return AppMessageType.fileIsAlreadyBeingPlayed;
    case 203:
      return AppMessageType.fileNotSupported;
    case 204:
      return AppMessageType.filesAreNotValid;
    case 205:
      return AppMessageType.noFilesToBeAdded;
    case 206:
      return AppMessageType.urlNotSupported;
    case 207:
      return AppMessageType.urlCouldntBeParsed;
    case 208:
      return AppMessageType.oneOrMoreFilesAreNotReadyYet;
    case 300:
      return AppMessageType.noDevicesFound;
    case 301:
      return AppMessageType.noInternetConnection;
    case 302:
      return AppMessageType.connectionToDeviceIsStillInProgress;
    case 303:
      return AppMessageType.ffmpegError;
    case 304:
      return AppMessageType.serverIsClosing;
    case 305:
      return AppMessageType.ffmpegExecutableNotFound;
  }
  throw Exception('The provided code = $value is not valid');
}
