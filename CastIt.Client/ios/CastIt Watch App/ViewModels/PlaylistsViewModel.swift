import Foundation
import Observation

@Observable
class PlaylistsViewModel {
    var playlists: [GetAllPlayListResponseDto] = []
    var busyPlaylists: [Int: Bool] = [:]

    private let signalRService: SignalRService

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
    }

    func getPlaylists() {
        signalRService.getPlaylists()
    }

    @MainActor
    func addNewPlayList() async {
        do {
            let newPlaylist = try await signalRService.addNewPlayList()
            // We rely on hub event PlayListAdded to update the summary list
        } catch {
            print("Error adding new playlist: \(error)")
        }
    }

    func deletePlayList(id: Int) {
        signalRService.deletePlayList(id: id)
    }
}
