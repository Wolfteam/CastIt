export enum AppMessage {
    unknownErrorOccurred = 1,
    invalidRequest = 2,
    notFound = 3,

    playListNotFound = 100,

    unknownErrorLoadingFile = 200,
    fileNotFound = 201,
    fileIsAlreadyBeingPlayed = 202,
    fileNotSupported = 203,
    filesAreNotValid = 204,
    noFilesToBeAdded = 205,
    urlNotSupported = 206,
    urlCouldntBeParsed = 207,
    oneOrMoreFilesAreNotReadyYet = 208,

    noDevicesFound = 300,
    noInternetConnection = 301,
    connectionToDeviceIsStillInProgress = 302,
    ffmpegError = 303,
    serverIsClosing = 304,
    ffmpegExecutableNotFound = 305,
}