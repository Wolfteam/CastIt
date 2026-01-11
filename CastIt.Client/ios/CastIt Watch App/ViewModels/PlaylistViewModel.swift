import Foundation
import Combine
import Observation

@Observable
class PlaylistViewModel {
    var id: Int?
    var name: String?
    var position: Int?
    var loop: Bool?
    var shuffle: Bool?
    var numberOfFiles: Int?
    var playedTime: String?
    var totalDuration: String?
    var imageUrl: String?
    var lastPlayedDate: String?

    var files: [FileItemResponseDto] = []
    var loadingFiles: [Int: Bool] = [:]
    var isLoading: Bool = false

    private var cancellables = Set<AnyCancellable>()
    private let signalRService: SignalRService

    init(signalRService: SignalRService, id: Int, name: String) {
        self.signalRService = signalRService
        self.id = id
        self.name = name
        // Subscribe to events relevant to the active playlist
        bind()
    }

    private func bind() {
        // When active playlist is set/changed, we don't rewire publishers here; we filter inside sinks
        signalRService.onFileAdded
            .receive(on: DispatchQueue.main)
            .sink { [weak self] newFile in
                guard let self, self.id == newFile.playListId else { return }
                self.files.append(newFile)
            }
            .store(in: &cancellables)

        signalRService.onFileChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updatedFile in
                guard let self, self.id == updatedFile.playListId else { return }
                if let idx = self.files.firstIndex(where: { $0.id == updatedFile.id }) {
                    self.files[idx] = updatedFile
                }
            }
            .store(in: &cancellables)

        signalRService.onFilesChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updatedFiles in
                guard let self, let first = updatedFiles.first, self.id == first.playListId else { return }
                self.files = updatedFiles
            }
            .store(in: &cancellables)

        signalRService.onFileDeleted
            .receive(on: DispatchQueue.main)
            .sink { [weak self] deleted in
                guard let self, self.id == deleted.playListId else { return }
                self.files.removeAll { $0.id == deleted.fileId }
            }
            .store(in: &cancellables)

        signalRService.onPlayListChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updated in
                guard let self, self.id != nil && self.id == updated.id else { return }
                self.update(from: updated)
            }
            .store(in: &cancellables)

        signalRService.onFileLoading
            .receive(on: DispatchQueue.main)
            .sink { [weak self] loading in
                self?.loadingFiles[loading.id] = true
            }
            .store(in: &cancellables)

        signalRService.onFileLoaded
            .receive(on: DispatchQueue.main)
            .sink { [weak self] loaded in
                self?.loadingFiles[loaded.id] = false
            }
            .store(in: &cancellables)
    }

    private func update(from playlist: GetAllPlayListResponseDto) {
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
    }

    @MainActor
    func loadPlaylist(id: Int) async {
        isLoading = true
        defer { isLoading = false }
        do {
            let playlist = try await signalRService.getPlayList(playlistId: id)
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
            
            self.files = playlist.files
        } catch {
            print("Error getting playlist \(id): \(error)")
        }
    }

    deinit {
        debugPrint("PlaylistViewModel deinit: \(id ?? -1) - \(name ?? "unknown")")
    }
}
