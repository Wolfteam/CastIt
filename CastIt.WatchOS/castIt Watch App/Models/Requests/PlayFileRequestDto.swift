import Foundation

// Mirrors ClientApp IPlayFileRequestDto
struct PlayFileRequestDto: Codable {
    let id: Int
    let playListId: Int
    let force: Bool
    let fileOptionsChanged: Bool
}
