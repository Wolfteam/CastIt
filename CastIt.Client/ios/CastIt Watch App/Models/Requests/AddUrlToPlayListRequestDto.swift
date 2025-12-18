import Foundation

// Mirrors ClientApp IAddUrlToPlayListRequestDto
struct AddUrlToPlayListRequestDto: Codable {
    let url: String
    let onlyVideo: Bool
}
