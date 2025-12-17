import Foundation

// Mirrors ClientApp src/enums/app_file.enum.ts
enum AppFile: Int, Codable {
    case na = 1         // 1 << 0
    case local = 2      // 1 << 1
    case url = 4        // 1 << 2
    case hls = 8        // 1 << 3
    case localVideo = 16
    case localMusic = 32
    case localSubtitle = 64
}
