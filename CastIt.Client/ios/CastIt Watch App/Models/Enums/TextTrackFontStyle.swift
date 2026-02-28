import Foundation

// Mirrors ClientApp src/enums/text_track_font_style.enum.ts
enum TextTrackFontStyle: Int, Codable {
    case normal = 0
    case bold = 1
    case boldItalic = 2
    case italic = 3
}
