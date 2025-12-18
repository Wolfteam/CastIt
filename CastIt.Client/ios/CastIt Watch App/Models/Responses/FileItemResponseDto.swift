import Foundation

// Mirrors ClientApp IFileItemResponseDto
struct FileItemResponseDto: Codable, Identifiable {
    let id: Int
    let name: String
    let description: String
    let totalSeconds: Double
    let path: String
    let position: Int
    let playedPercentage: Double
    let playListId: Int
    let loop: Bool
    let lastPlayedDate: String?

    let isBeingPlayed: Bool
    //let type: AppFile
    let isLocalFile: Bool
    let isUrlFile: Bool
    let playedSeconds: Double
    let canStartPlayingFromCurrentPercentage: Bool
    let wasPlayed: Bool
    let isCached: Bool

    let exists: Bool
    let filename: String
    let size: String
    let `extension`: String

    let subTitle: String
    let resolution: String?
    let duration: String
    let playedTime: String
    let totalDuration: String
    let fullTotalDuration: String
    let thumbnailUrl: String?

    let currentFileVideos: [FileItemOptionsResponseDto]
    let currentFileAudios: [FileItemOptionsResponseDto]
    let currentFileSubTitles: [FileItemOptionsResponseDto]
    let currentFileQualities: [FileItemOptionsResponseDto]
    let currentFileVideoStreamIndex: Int
    let currentFileAudioStreamIndex: Int
    let currentFileSubTitleStreamIndex: Int
    let currentFileQuality: Int
}
