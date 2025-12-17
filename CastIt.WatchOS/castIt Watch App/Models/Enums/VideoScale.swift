import Foundation

// Mirrors ClientApp src/enums/video_scale.enum.ts
enum VideoScale: Int, Codable {
    case original = 1
    case hd = 720
    case fullHd = 1080
}
