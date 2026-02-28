import Foundation

// Mirrors ClientApp IServerAppSettings
struct ServerAppSettings: Codable {
    let startFilesFromTheStart: Bool
    let playNextFileAutomatically: Bool
    let forceVideoTranscode: Bool
    let forceAudioTranscode: Bool
    let videoScale: VideoScale
    let enableHardwareAcceleration: Bool
    let webVideoQuality: Int

    let currentSubtitleFgColor: SubtitleFgColor
    let currentSubtitleBgColor: SubtitleBgColor
    let currentSubtitleFontScale: SubtitleFontScale
    let currentSubtitleFontStyle: TextTrackFontStyle
    let currentSubtitleFontFamily: TextTrackFontGenericFamily
    let subtitleDelayInSeconds: Double
    let loadFirstSubtitleFoundAutomatically: Bool
}
