import Foundation
import Observation

@MainActor
protocol DependencyContainer {
    // Shared ViewModels
    var playerViewModel: PlayerViewModel { get }
    var settingsViewModel: SettingsViewModel { get }
    var playlistsViewModel: PlaylistsViewModel { get }

    // App routing
    var router: AppRouter { get }

    // Factories (non-shared)
    func makePlaylistViewModel() -> PlaylistViewModel
    func makeFileItemViewModel(file: FileItemResponseDto) -> FileItemViewModel

    // Low-level services (exposed only if strictly necessary)
    var signalRService: SignalRService { get }
}

@MainActor
final class AppContainer: DependencyContainer {
    // Services
    let signalRService: SignalRService
    let router: AppRouter

    // Shared VMs
    let playerViewModel: PlayerViewModel
    let settingsViewModel: SettingsViewModel
    let playlistsViewModel: PlaylistsViewModel

    // Internal wiring helper
    private let viewModelService: ViewModelService

    init(serverUrl: String? = nil) {
        // In the future we can pass serverUrl to SignalRService
        self.signalRService = SignalRService()
        self.router = AppRouter()

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

    func makeFileItemViewModel(file: FileItemResponseDto) -> FileItemViewModel {
        FileItemViewModel(signalRService: signalRService, file: file)
    }
}
