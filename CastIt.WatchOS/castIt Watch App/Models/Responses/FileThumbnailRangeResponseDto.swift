import Foundation

// Mirrors ClientApp IFileThumbnailRangeResponseDto
struct FileThumbnailRangeResponseDto: Codable {
    let previewThumbnailUrl: String
    let thumbnailRange: NumericRange
    let thumbnailPositions: [FileThumbnailPositionResponseDto]
}

struct FileThumbnailPositionResponseDto: Codable {
    let x: Int
    let y: Int
    let second: Double
}

struct NumericRange: Codable {
    let minimum: Double
    let maximum: Double
}
