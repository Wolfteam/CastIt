import Foundation
import Observation

@Observable
final class AppRouter {
    enum Tab: Hashable {
        case player
        case playlists
        case settings
    }

    var selectedTab: Tab = .player
    var selectedPlaylist: GetAllPlayListResponseDto? = nil
}
