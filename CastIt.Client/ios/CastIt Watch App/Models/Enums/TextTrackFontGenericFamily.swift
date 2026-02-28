import Foundation

// Mirrors ClientApp src/enums/text_track_font_generic_family.enum.ts
enum TextTrackFontGenericFamily: Int, Codable {
    case sansSerif = 0
    case monospacedSansSerif = 1
    case serif = 2
    case monospacedSerif = 3
    case casual = 4
    case cursive = 5
    case smallCapitals = 6
}
