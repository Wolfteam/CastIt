import Foundation

// Mirrors ClientApp IPlayListItemResponseDto
struct PlayListItemResponseDto: Codable, Identifiable {
    // From IGetAllPlayListResponseDto
    let id: Int
    let name: String
    let position: Int
    let loop: Bool
    let shuffle: Bool
    let numberOfFiles: Int
    let playedTime: String
    let totalDuration: String
    let imageUrl: String
    let lastPlayedDate: String?

    // Additional
    let files: [FileItemResponseDto]
}
