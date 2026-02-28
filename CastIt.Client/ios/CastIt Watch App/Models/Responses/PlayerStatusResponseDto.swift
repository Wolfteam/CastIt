import Foundation

// Mirrors ClientApp IPlayerStatusResponseDto
struct PlayerStatusResponseDto: Codable {
    let mrl: String?
    let isPlaying: Bool
    let isPaused: Bool
    let isPlayingOrPaused: Bool

    let currentMediaDuration: Double
    let elapsedSeconds: Double
    let playedPercentage: Double

    let volumeLevel: Double
    let isMuted: Bool
}
