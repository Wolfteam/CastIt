import Foundation

// Mirrors ClientApp src/enums/sort_mode.enum.ts
enum SortMode: Int, Codable {
    case alphabeticalPathAsc = 0
    case alphabeticalPathDesc = 1
    case alphabeticalFileAsc = 2
    case alphabeticalFileDesc = 3
    case durationAsc = 4
    case durationDesc = 5
    case recentlyPlayed = 6
}
