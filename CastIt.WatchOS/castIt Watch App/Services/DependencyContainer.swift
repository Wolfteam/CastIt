import Foundation
import Observation

@MainActor
protocol DependencyContainer {
    // Shared ViewModels
    var playerViewModel: PlayerViewModel { get }
    var settingsViewModel: SettingsViewModel { get }
    var playlistsViewModel: PlaylistsViewModel { get }

    // Factories (non-shared)
    func makePlaylistViewModel() -> PlaylistViewModel

    // Low-level services (exposed only if strictly necessary)
    var signalRService: SignalRService { get }
}

@MainActor
final class AppContainer: DependencyContainer {
    // Services
    let signalRService: SignalRService

    // Shared VMs
    let playerViewModel: PlayerViewModel
    let settingsViewModel: SettingsViewModel
    let playlistsViewModel: PlaylistsViewModel

    // Internal wiring helper
    private let viewModelService: ViewModelService

    init(serverUrl: String? = nil) {
        // In the future we can pass serverUrl to SignalRService
        self.signalRService = SignalRService()

        // Create shared VMs
        self.playerViewModel = PlayerViewModel(signalRService: signalRService)
        self.settingsViewModel = SettingsViewModel(signalRService: signalRService)
        self.playlistsViewModel = PlaylistsViewModel(signalRService: signalRService)

        // Wire SignalR → shared VMs
        self.viewModelService = ViewModelService(
            signalRService: signalRService,
            playerViewModel: playerViewModel,
            playlistsViewModel: playlistsViewModel,
            settingsViewModel: settingsViewModel
        )

        // Start connection by default
        self.signalRService.connect()
    }

    func makePlaylistViewModel() -> PlaylistViewModel {
        PlaylistViewModel(signalRService: signalRService)
    }
}
