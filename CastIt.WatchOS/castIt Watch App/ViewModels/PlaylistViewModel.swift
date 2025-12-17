import Foundation
import Combine
import Observation

@Observable
class PlaylistViewModel {
    var playlist: GetAllPlayListResponseDto?
    var files: [FileItemResponseDto] = []
    var loadingFiles: [Int: Bool] = [:]

    private var cancellables = Set<AnyCancellable>()
    private let signalRService: SignalRService

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
        // Subscribe to events relevant to the active playlist
        bind()
    }

    private func bind() {
        // When active playlist is set/changed, we don't rewire publishers here; we filter inside sinks
        signalRService.onFileAdded
            .receive(on: DispatchQueue.main)
            .sink { [weak self] newFile in
                guard let self, self.playlist?.id == newFile.playListId else { return }
                self.files.append(newFile)
            }
            .store(in: &cancellables)

        signalRService.onFileChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updatedFile in
                guard let self, self.playlist?.id == updatedFile.playListId else { return }
                if let idx = self.files.firstIndex(where: { $0.id == updatedFile.id }) {
                    self.files[idx] = updatedFile
                }
            }
            .store(in: &cancellables)

        signalRService.onFilesChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updatedFiles in
                guard let self, let first = updatedFiles.first, self.playlist?.id == first.playListId else { return }
                self.files = updatedFiles
            }
            .store(in: &cancellables)

        signalRService.onFileDeleted
            .receive(on: DispatchQueue.main)
            .sink { [weak self] deleted in
                guard let self, self.playlist?.id == deleted.playListId else { return }
                self.files.removeAll { $0.id == deleted.fileId }
            }
            .store(in: &cancellables)

        signalRService.onPlayListChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] updated in
                guard let self, self.playlist != nil && self.playlist?.id == updated.id else { return }
                self.playlist = updated
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

    @MainActor
    func getPlaylist(id: Int) async {
        do {
            let playlist = try await signalRService.getPlayList(playlistId: id)
            self.playlist = GetAllPlayListResponseDto(
                id: playlist.id,
                name: playlist.name,
                position: playlist.position,
                loop: playlist.loop,
                shuffle: playlist.shuffle,
                numberOfFiles: playlist.numberOfFiles,
                playedTime: playlist.playedTime,
                totalDuration: playlist.totalDuration,
                imageUrl: playlist.imageUrl,
                lastPlayedDate: playlist.lastPlayedDate
            )
            self.files = playlist.files
        } catch {
            print("Error getting playlist \(id): \(error)")
        }
    }
    
    func play(file: FileItemResponseDto) {
        signalRService.play(playListId: file.playListId, fileId: file.id)
    }
}
