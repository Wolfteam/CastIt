import Foundation

// Mirrors ClientApp ISetFileOptionsRequestDto
struct SetFileOptionsRequestDto: Codable {
    let streamIndex: Int
    let isAudio: Bool
    let isSubTitle: Bool
    let isQuality: Bool
}
