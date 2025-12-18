import Foundation
import Combine
import Observation

@Observable
class PlayerViewModel {
    var status: ServerPlayerStatusResponseDto?
    var settings: ServerAppSettings?
    var devices: [Receiver] = []
    var selectedDevice: Receiver?

    // UI state
    var isLoading: Bool = false
    var isConnected: Bool = false

    // Derived state
    var isPlayingOrPaused: Bool { status?.player.isPlayingOrPaused ?? false }
    var isPlaying: Bool { status?.player.isPlaying ?? false }
    var playedPercentage: Double { status?.player.playedPercentage ?? 0 }

    private let signalRService: SignalRService
    private var cancellables = Set<AnyCancellable>()

    init(signalRService: SignalRService) {
        self.signalRService = signalRService
        bind()
    }

    func togglePlayBack() {
        signalRService.togglePlayBack()
    }

    func goTo(next: Bool, previous: Bool) {
        signalRService.goTo(next: next, previous: previous)
    }
    
    func stop() {
        signalRService.stopPlayBack()
    }

    private func bind() {
        // Loading states for player
        signalRService.onFileLoading
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.isLoading = true }
            .store(in: &cancellables)

        signalRService.onFileLoaded
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.isLoading = false }
            .store(in: &cancellables)

        signalRService.onStoppedPlayBack
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in self?.isLoading = false }
            .store(in: &cancellables)

        // End reached – keep percentage at 100% if possible
        signalRService.onFileEndReached
            .receive(on: DispatchQueue.main)
            .sink { [weak self] file in
                guard let self, let current = self.status?.playedFile, current.id == file.id else { return }
                // Force an update by touching status; actual percentage will come from server soon
                if var s = self.status { self.status = s }
            }
            .store(in: &cancellables)

        // Connection status
        signalRService.onClientConnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in self?.isConnected = true }
            .store(in: &cancellables)

        signalRService.onClientDisconnected
            .receive(on: DispatchQueue.main)
            .sink { [weak self] in self?.isConnected = false }
            .store(in: &cancellables)
    }
}
