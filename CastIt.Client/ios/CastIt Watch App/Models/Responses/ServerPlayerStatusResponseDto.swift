import Foundation

// Mirrors ClientApp IServerPlayerStatusResponseDto
struct ServerPlayerStatusResponseDto: Codable {
    let player: PlayerStatusResponseDto
    let playList: GetAllPlayListResponseDto?
    let playedFile: FileItemResponseDto?
    let thumbnailRanges: [FileThumbnailRangeResponseDto]
}
