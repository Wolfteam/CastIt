import Foundation

// Mirrors ClientApp ISetVolumeRequestDto
struct SetVolumeRequestDto: Codable {
    let isMuted: Bool
    let volumeLevel: Double
}
