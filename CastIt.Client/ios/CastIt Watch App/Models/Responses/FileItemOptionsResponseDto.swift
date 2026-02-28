import Foundation

// Mirrors ClientApp IFileItemOptionsResponseDto
struct FileItemOptionsResponseDto: Codable, Identifiable {
    let id: Int
    let isVideo: Bool
    let isAudio: Bool
    let isSubTitle: Bool
    let isQuality: Bool
    let path: String?
    let text: String
    let isSelected: Bool
    let isEnabled: Bool
}
