import Foundation

public enum AppMessage: Int, Codable {
    case unknownErrorOccurred = 1
    case invalidRequest = 2
    case notFound = 3

    case playListNotFound = 100

    case unknownErrorLoadingFile = 200
    case fileNotFound = 201
    case fileIsAlreadyBeingPlayed = 202
    case fileNotSupported = 203
    case filesAreNotValid = 204
    case noFilesToBeAdded = 205
    case urlNotSupported = 206
    case urlCouldntBeParsed = 207
    case oneOrMoreFilesAreNotReadyYet = 208

    case noDevicesFound = 300
    case noInternetConnection = 301
    case connectionToDeviceIsStillInProgress = 302
    case ffmpegError = 303
    case serverIsClosing = 304
    case ffmpegExecutableNotFound = 305
}

extension AppMessage {
    var localizedDescription: String {
        switch self {
        case .unknownErrorOccurred:
            return "Unknown error occurred"
        case .invalidRequest:
            return "Invalid request"
        case .notFound:
            return "The resource you were looking for does not exist"
        case .playListNotFound:
            return "PlayList not found"
        case .unknownErrorLoadingFile:
            return "Unknown error occurred while trying to play file"
        case .fileNotFound:
            return "File not found"
        case .fileIsAlreadyBeingPlayed:
            return "File is already being played"
        case .fileNotSupported:
            return "One or more files are not supported"
        case .filesAreNotValid:
            return "One or more files are not valid"
        case .noFilesToBeAdded:
            return "No files to be added"
        case .urlNotSupported:
            return "Url is not supported"
        case .urlCouldntBeParsed:
            return "Url could not be parsed"
        case .oneOrMoreFilesAreNotReadyYet:
            return "One or more files are not ready yet"
        case .noDevicesFound:
            return "No devices found"
        case .noInternetConnection:
            return "No internet connection"
        case .connectionToDeviceIsStillInProgress:
            return "Connection to device is still in progress"
        case .ffmpegError:
            return "Ffmpeg error"
        case .serverIsClosing:
            return "Server is closing"
        case .ffmpegExecutableNotFound:
            return "Ffmpeg executable not found"
        }
    }
}
