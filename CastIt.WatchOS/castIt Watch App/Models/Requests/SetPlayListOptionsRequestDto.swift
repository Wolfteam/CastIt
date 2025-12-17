import Foundation

// Mirrors ClientApp ISetPlayListOptionsRequestDto
struct SetPlayListOptionsRequestDto: Codable {
    let loop: Bool
    let shuffle: Bool
}
