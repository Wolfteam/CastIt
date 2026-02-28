import Foundation
import Combine
import Observation

@Observable
class PlaylistItemViewModel {
    var id: Int
    var name: String
    var position: Int
    var loop: Bool
    var shuffle: Bool
    var numberOfFiles: Int
    var playedTime: String
    var totalDuration: String
    var imageUrl: String?
    var lastPlayedDate: String?

    var showRename: Bool = false
    var showDeleteConfirmation: Bool = false
    var newName: String = ""

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService, playlist: GetAllPlayListResponseDto) {
        self.signalRService = signalRService
        
        self.id = playlist.id
        self.name = playlist.name
        self.position = playlist.position
        self.loop = playlist.loop
        self.shuffle = playlist.shuffle
        self.numberOfFiles = playlist.numberOfFiles
        self.playedTime = playlist.playedTime
        self.totalDuration = playlist.totalDuration
        self.imageUrl = playlist.imageUrl
        self.lastPlayedDate = playlist.lastPlayedDate
        self.newName = playlist.name

        bind()
    }

    private func bind() {
        signalRService.onPlayListChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updatedPlaylist in
                guard let self = self, updatedPlaylist.id == self.id else { return }
                self.update(from: updatedPlaylist)
            }
            .store(in: &cancellables)

        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .compactMap { $0.playList }
            .sink { [weak self] updatedPlaylist in
                guard let self = self, updatedPlaylist.id == self.id else { return }
                self.update(from: updatedPlaylist)
            }
            .store(in: &cancellables)
    }

    private func update(from playlist: GetAllPlayListResponseDto) {
        self.name = playlist.name
        self.position = playlist.position
        self.loop = playlist.loop
        self.shuffle = playlist.shuffle
        self.numberOfFiles = playlist.numberOfFiles
        self.playedTime = playlist.playedTime
        self.totalDuration = playlist.totalDuration
        self.imageUrl = playlist.imageUrl
        self.lastPlayedDate = playlist.lastPlayedDate
    }

    func toggleLoop() {
        signalRService.setPlayListOptions(playListId: id, loop: !loop, shuffle: shuffle)
    }

    func toggleShuffle() {
        signalRService.setPlayListOptions(playListId: id, loop: loop, shuffle: !shuffle)
    }

    func rename(newName: String) {
        signalRService.updatePlayList(id: id, name: newName)
    }

    func delete() {
        signalRService.deletePlayList(id: id)
    }

    deinit {
        debugPrint("PlaylistItemViewModel deinit: \(id) - \(name)")
    }
}
