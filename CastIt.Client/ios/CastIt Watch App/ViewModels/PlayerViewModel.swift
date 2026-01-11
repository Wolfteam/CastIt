import Foundation
import Combine
import Observation
import SwiftUI

@Observable
class PlayerViewModel {
    var player: PlayerStatusResponseDto?
    var playList: GetAllPlayListResponseDto?
    var playedFile: FileItemResponseDto?
    var thumbnailRanges: [FileThumbnailRangeResponseDto] = []

    var settings: ServerAppSettings?
    var devices: [Receiver] = []
    var selectedDevice: Receiver?

    // UI state
    var isLoading: Bool = false
    var isConnected: Bool = false

    var showMore: Bool = false
    var showServerAlert: Bool = false
    var serverAlertText: String = ""
    var backgroundColors: [Color] = []

    // Derived state
    var isPlayingOrPaused: Bool { player?.isPlayingOrPaused ?? false }
    var isPlaying: Bool { player?.isPlaying ?? false }
    var playedPercentage: Double { player?.playedPercentage ?? 0 }

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

    private func updateBackgroundColors() {
        guard let thumbnailUrl = playedFile?.thumbnailUrl, let url = URL(string: thumbnailUrl) else {
            self.backgroundColors = []
            return
        }

        Task {
            do {
                let (data, _) = try await URLSession.shared.data(from: url)
                let colors = DominantColorsExtractor.dominantColors(from: data)
                await MainActor.run {
                    self.backgroundColors = colors
                }
            } catch {
                await MainActor.run {
                    self.backgroundColors = []
                }
            }
        }
    }

    private func bind() {
        // Player status changed
        signalRService.onPlayerStatusChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] status in
                guard let self else { return }
                self.player = status.player
                self.playList = status.playList
                let oldPlayedFile = self.playedFile
                self.playedFile = status.playedFile
                self.thumbnailRanges = status.thumbnailRanges
                
                if oldPlayedFile?.thumbnailUrl != self.playedFile?.thumbnailUrl {
                    self.updateBackgroundColors()
                }
            }
            .store(in: &cancellables)

        // Settings changed
        signalRService.onPlayerSettingsChanged
            .receive(on: DispatchQueue.main)
            .sink { [weak self] settings in
                self?.settings = settings
            }
            .store(in: &cancellables)

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
                guard let self, let current = self.playedFile, current.id == file.id else { return }
                // Force an update by touching status; actual percentage will come from server soon
                // Since we flattened it, we might need to manually update player percentage if we want immediate UI feedback
                if var p = self.player {
                    self.player = PlayerStatusResponseDto(
                        mrl: p.mrl,
                        isPlaying: p.isPlaying,
                        isPaused: p.isPaused,
                        isPlayingOrPaused: p.isPlayingOrPaused,
                        currentMediaDuration: p.currentMediaDuration,
                        elapsedSeconds: p.elapsedSeconds,
                        playedPercentage: 100,
                        volumeLevel: p.volumeLevel,
                        isMuted: p.isMuted
                    )
                }
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
